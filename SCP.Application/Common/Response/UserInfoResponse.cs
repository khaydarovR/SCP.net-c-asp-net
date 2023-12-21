namespace SCP.Api.DTO
{
    public class UserInfoResponse
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public bool FA2Enabled { get; set; }
        public bool EmailVerified { get; set; }
        public string Jwt { get; set; }
    }

}
