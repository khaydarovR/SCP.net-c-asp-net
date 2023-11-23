namespace SCP.Application.Common.Response
{
    public class GetUserResponse
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; }
    }
}
