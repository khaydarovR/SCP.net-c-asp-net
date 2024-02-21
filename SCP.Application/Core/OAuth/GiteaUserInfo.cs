namespace SCP.Application.Core.OAuth
{
    public class GiteaUserInfo
    {
        public int id { get; set; }
        public string login { get; set; }
        public string login_name { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string avatar_url { get; set; }
        public string language { get; set; }
        public bool is_admin { get; set; }
        public DateTime last_login { get; set; }
        public DateTime created { get; set; }
        public bool restricted { get; set; }
        public bool active { get; set; }
        public bool prohibit_login { get; set; }
        public string location { get; set; }
        public string website { get; set; }
        public string description { get; set; }
        public string visibility { get; set; }
        public int followers_count { get; set; }
        public int following_count { get; set; }
        public int starred_repos_count { get; set; }
        public string username { get; set; }
    }
}
