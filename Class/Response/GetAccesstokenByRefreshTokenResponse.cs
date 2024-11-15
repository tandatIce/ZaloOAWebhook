namespace ZaloOAWebhook.Class.Response
{
    public class GetAccesstokenByRefreshTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? expires_in { get; set; }
        public string? error_name { get; set; }
        public string? error_reason { get; set; }
        public string? ref_doc { get; set; }
        public string? error_description { get; set; }
        public int? error { get; set; }
    }
}
