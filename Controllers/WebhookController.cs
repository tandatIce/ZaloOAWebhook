
using Microsoft.AspNetCore.Mvc;
using ZaloOAWebhook.Constant;
using ZaloOAWebhook.Models;
using Unidecode.NET;
using Microsoft.Data.SqlClient;
using RestSharp;
using Newtonsoft.Json;
using ZaloOAWebhook.Class.Response;
using ZaloOAWebhook.Class.Request;
using ZaloOAWebhook.IRepository;


namespace ZaloOAWebhook.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IZaloOAAccountRepository _ZaloOAAccountRepo;
        private readonly ICustomerRepoitory _customerRepo;
        private readonly IConfiguration _config;

        private string _errorMessage;
        public WebhookController(ILogger<WebhookController> logger, IZaloOAAccountRepository zaloOAAccountRepo, ICustomerRepoitory customerRepo
                                , IConfiguration config)
        {
            _logger = logger;
            _errorMessage = "";
            _ZaloOAAccountRepo = zaloOAAccountRepo;
            _customerRepo = customerRepo;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload, [FromHeader(Name = "X-ZEvent-Signature")] string signature)
        {
            _logger.LogInformation("payload: " + payload.ToString());
            _logger.LogInformation("signature : " + signature);

            if (payload == null)
            {
                return BadRequest(new Response<string>(400, false, MessageReponse.FAIL, 400, "Invalid payload"));
            }

            // Lấy thông tin từ payload
            string appId = payload.app_id;
            string timestamp = payload.timestamp;
            string eventName = payload.event_name;


            //handle when user Zalo send message Zalo OA
            if (eventName == EventName.USER_SEND_TEXT)
            {
                string userId = payload?.sender?.id ?? string.Empty;
                string messeageText = payload?.message?.text ?? string.Empty;

                IEnumerable<ZaloOaAccount> OAAccount = Enumerable.Empty<ZaloOaAccount>();

                try
                {
                    OAAccount = await _ZaloOAAccountRepo.GetAccountByAppId(appId);
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, new Response<string>(500, false, MessageReponse.FAIL, 500, "Error SQL", "", "", ex.Message));
                }


                if (OAAccount == null || !OAAccount.Any())
                {
                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, "Can not find Account OA"));
                }

                if (OAAccount.Count() > 1)
                {
                    return BadRequest(new Response<string>(400, false, MessageReponse.FAIL, 400, "More than one Zalo OA account found"));
                }

                /*
                 handle valid signature
                 */

                var account = OAAccount.ToList()[0];

                string accessToken = account.AccessToken;
                string refreshToken = account.RefreshToken;
                string secretKey = account.Secret_key;
                string databaseName = account.DatabaseName;
                string textRequest = messeageText.Unidecode().ToLower();
                string textBatDau = "Bắt đầu".Unidecode().ToLower();

                if (textRequest.Equals(textBatDau))  //case Zalo user send "Bat Dat" to Zalo OA, handle Update userID customer
                {

                    bool result = await UpdateUserIdCustomer(userId, accessToken, refreshToken, secretKey, appId, databaseName);
                    if (result) return Ok();

                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }

                return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, "No messages match"));

            }

            return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, "No events match"));
        }

        private async Task<bool> UpdateUserIdCustomer(string userId, string accesstoken, string refreshToken, string secretKey, string appId,
                                                    string databaseName)
        {
            UserData? inforCustomer = await GetDetailInforCustomer(userId, accesstoken, refreshToken, secretKey, appId);

            if (inforCustomer == null) return false;

            //update userID by phone Number
            string phoneNumber = inforCustomer.shared_info.phone;

            try
            {
                await _customerRepo.updateUserIdByPhoneNumber(phoneNumber, databaseName, userId);
            }
            catch (SqlException ex)
            {
                _errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        private async Task<UserData?> GetDetailInforCustomer(string userId, string accesstoken, string refreshToken, string secretKey,
                                                            string appId)
        {
            var client = new RestClient(_config["APIZaloOAUrl:GetDetailInforCustomer"] + "?data={\"user_id\":\"" + userId + "\"}");
            var request = new RestRequest("", Method.Get);

            request.AddHeader("access_token", accesstoken);
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var responseConvert = JsonConvert.DeserializeObject<DetailInforCustomerResponse>(response.Content ?? string.Empty);

                if (responseConvert?.error == 0)
                {   //case call api get infor success
                    return responseConvert.data;
                }

                _logger.LogError($"Error: {responseConvert?.error} - {responseConvert?.message}");

                if (responseConvert?.error == -216)
                {   //case access token not valid

                    string newAccessToken = await GetAccessTokenByRefreshToken(refreshToken, secretKey, appId);

                    if (newAccessToken == string.Empty) return null;

                    return await GetDetailInforCustomer(userId, newAccessToken, "", "", "");

                }

                _errorMessage = responseConvert?.message ?? string.Empty;
                return null;
            }

            _logger.LogError($"Error: {response.StatusCode} - {response.StatusDescription}");
            _errorMessage = response.StatusDescription ?? string.Empty;
            return null;

        }

        private async Task<string> GetAccessTokenByRefreshToken(string refreshToken, string secretKey, string appId)
        {
            var client = new RestClient(_config["APIZaloOAUrl:GetAccessTokenByRefreshToken"] ?? string.Empty);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("secret_key", secretKey);
            request.AddJsonBody(new
            {
                refresh_token = "",
                app_id = "",
                grant_type = ""
            });

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var responseConvert = JsonConvert.DeserializeObject<GetAccesstokenByRefreshTokenResponse>(response.Content ?? String.Empty);

                if (responseConvert?.error != null)
                {
                    _errorMessage = responseConvert?.error_name + ", " + responseConvert?.error_reason ?? string.Empty;
                    return string.Empty;
                }

                string newAccessToken = responseConvert?.access_token ?? string.Empty;
                string newRefreshToken = responseConvert?.refresh_token ?? string.Empty;

                //update accesstoken, refresstoken
                try
                {
                    await _ZaloOAAccountRepo.UpdateRefeshAndAccessTokenByAppId(appId, newAccessToken, newRefreshToken);
                }
                catch (SqlException ex)
                {
                    _errorMessage = ex.Message;
                    return string.Empty;
                }

                return newAccessToken ?? string.Empty;

            }

            _logger.LogError($"Error: {response.StatusCode} - {response.StatusDescription}");
            _errorMessage = response.StatusDescription ?? string.Empty;
            return "";

        }

    }
}
