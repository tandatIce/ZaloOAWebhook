﻿
using Microsoft.AspNetCore.Mvc;
using ZaloOAWebhook.Constant;
using ZaloOAWebhook.Models;
using Unidecode.NET;
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

                OAAccount = await _ZaloOAAccountRepo.GetAccountByAppId(appId);

                if (OAAccount == null || !OAAccount.Any())
                {
                    _errorMessage = "Can not find Account OA";
                    _logger.LogError(_errorMessage);
                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }

                if (OAAccount.Count() > 1)
                {
                    _errorMessage = "More than one Zalo OA account found";
                    _logger.LogError(_errorMessage);
                    return BadRequest(new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }

                /*
                 handle valid signature
                 */

                var account = OAAccount.ToList()[0];

                string accessToken = account.AccessToken;
                string refreshToken = account.RefreshToken;
                string secretKey = account.Secret_key;
                string databaseName = account.DatabaseName;
                DateTime updateDate = account.UpdateDate;
                string textRequest = messeageText.Unidecode().ToLower();
                string textBatDau = MessageSentFromUser.BAT_DAU.Unidecode().ToLower();
                string textDiem = MessageSentFromUser.DIEM.Unidecode().ToLower();

                accessToken = await CheckExpireAccessToken(updateDate, accessToken, refreshToken, secretKey, appId);

                if (accessToken == string.Empty)
                {
                    _logger.LogError(_errorMessage);
                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }

                if (textRequest.Equals(textBatDau))  //case Zalo user send "Bat Dat" to Zalo OA, handle Update userID customer
                {

                    bool result = await UpdateUserIdCustomer(userId, accessToken, databaseName);
                    if (result) return Ok();

                    _logger.LogError(_errorMessage);
                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }

                if (textRequest.Equals(textDiem))  //case Zalo user send "Diem" to Zalo OA, handle send point to ZaloUser
                {
                    return await SendPointToCustomer(accessToken, userId, databaseName);
                }


                return Ok();

            }
            _errorMessage = "No events match";
            _logger.LogError(_errorMessage);
            return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
        }

        private async Task<bool> UpdateUserIdCustomer(string userId, string accesstoken, string databaseName)
        {
            UserData? inforCustomer = await GetDetailInforCustomer(userId, accesstoken);

            if (inforCustomer == null) return false;

            //update userID by phone Number
            string phoneNumber = inforCustomer.shared_info.phone;

            await _customerRepo.updateUserIdByPhoneNumber(phoneNumber, databaseName, userId);

            return true;
        }
        private async Task<IActionResult> SendPointToCustomer(string accessToken, string userId, string databaseName)
        {
            double point = 0;
            //get point from database
            Point? pointRow = await _customerRepo.GetPointCurrentByUserId(userId, databaseName);

            if (pointRow == null)
            {
                _errorMessage = "Can not find data about point";
                _logger.LogError(_errorMessage);
                return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
            }

            point = pointRow.P_SAVE ?? 0;

            var client = new RestClient(_config["APIZaloOAUrl:SendAdviseTextMessage"] ?? string.Empty);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("access_token", accessToken);

            RecipientAdviseMessage recipient = new() { user_id = userId };
            AdviseMessage adviseMessage = new() { text = $"Số điểm của quý khách hiện tại là: {point}" };
            AdviseMessageRequest bodyRequest=new() { message = adviseMessage ,recipient=recipient};

            request.AddJsonBody(bodyRequest);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var responseConvert = JsonConvert.DeserializeObject<SendAdviseMessageAPIResponse>(response.Content ?? String.Empty);

                if (responseConvert?.error != 0)
                {
                    _errorMessage = responseConvert?.error + ", " + responseConvert?.message ?? string.Empty;
                    _logger.LogError(_errorMessage);
                    return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, _errorMessage));
                }
                return Ok();
            }

            _logger.LogError($"{response.StatusCode} - {response.StatusDescription}");
            return StatusCode(400, new Response<string>(400, false, MessageReponse.FAIL, 400, response.StatusDescription ?? string.Empty));
        }
        private async Task<UserData?> GetDetailInforCustomer(string userId, string accesstoken)
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

                _errorMessage = responseConvert?.message ?? string.Empty;
                return null;
            }

            _logger.LogError($"Error: {response.StatusCode} - {response.StatusDescription}");
            _errorMessage = response.StatusDescription ?? string.Empty;
            return null;

        }
        private async Task<string> CheckExpireAccessToken(DateTime updateDate, string accessToken, string refreshToken, string secretKey, string appId)
        {
            DateTime now = DateTime.Now;
            TimeSpan duration = now - updateDate;
            double totalHours = duration.TotalHours;

            if (totalHours < 25) { return accessToken; }
            // case access token expire
            string newAccessToken = await GetAccessTokenByRefreshToken(refreshToken, secretKey, appId);
            return newAccessToken;
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

                await _ZaloOAAccountRepo.UpdateRefeshAndAccessTokenByAppId(appId, newAccessToken, newRefreshToken);

                return newAccessToken ?? string.Empty;

            }

            _logger.LogError($"Error: {response.StatusCode} - {response.StatusDescription}");
            _errorMessage = response.StatusDescription ?? string.Empty;
            return "";

        }

    }
}
