using System.Reflection;

namespace ZaloOAWebhook.Class.Request
{
    public class WebhookPayload
    {
        public string app_id { get; set; } = "";
        public Sender? sender { get; set; }
        public string user_id_by_app { get; set; } = "";
        public Recipient? recipient { get; set; }
        public string event_name { get; set; } = "";
        public Messages? message { get; set; }
        public string timestamp { get; set; } = "";
        public override string ToString()
        {
            return $"app_id: {app_id}, senderId: {sender?.id}, user_id_by_app: {user_id_by_app}, recipientId: {recipient?.id}, " +
                     $"event_name: {event_name}, message: {message?.text}, timestamp: {timestamp}";
        }
    }

    public class Sender
    {
        public string id { get; set; } = "";
    }

    public class Recipient
    {
        public string id { get; set; } = "";
    }

    public class Messages
    {
        public string text { get; set; } = "";
        public string msg_id { get; set; } = "";
    }
}
