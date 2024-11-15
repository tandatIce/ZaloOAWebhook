namespace ZaloOAWebhook.Class.Request
{
    public class AdviseMessageRequest
    {
        public RecipientAdviseMessage recipient { get; set; }
        public AdviseMessage message { get; set; }
    }

    public class RecipientAdviseMessage
    {
        public string user_id { get; set; } = string.Empty;
    }

    public class AdviseMessage
    {
        public string text { get; set; } = string.Empty;
    }
}
