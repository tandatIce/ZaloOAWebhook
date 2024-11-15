namespace ZaloOAWebhook.Class.Response
{

    public class UserData
    {
        public string user_id { get; set; } = null!;
        public string user_id_by_app { get; set; } = null!;
        public string user_external_id { get; set; } = null!;
        public string display_name { get; set; } = null!;
        public string user_alias { get; set; } = null!;
        public bool is_sensitive { get; set; }
        public string user_last_interaction_date { get; set; } = null!;
        public bool user_is_follower { get; set; }
        public string avatar { get; set; } = null!;
        public Avatars avatars { get; set; } = null!;
        public TagsAndNotesInfo tags_and_notes_info { get; set; } = null!;
        public SharedInfo shared_info { get; set; } = null!;
    }

    public class Avatars
    {
        public string _120 { get; set; } = null!;
        public string _240 { get; set; } = null!;
    }

    public class TagsAndNotesInfo
    {
        public string[] notes { get; set; } = null!;
        public string[] tag_names { get; set; } = null!;
    }

    public class SharedInfo
    {
        public string address { get; set; } = null!;
        public string city { get; set; } = null!;
        public string district { get; set; } = null!;
        public string phone { get; set; } = null!;
        public string name { get; set; } = null!;
    }

    public class DetailInforCustomerResponse
    {
        public UserData data { get; set; } = null!;
        public int error { get; set; }
        public string message { get; set; } = null!;
    }
}
