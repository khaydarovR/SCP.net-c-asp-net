namespace SCP.Application.Core.UserAuth
{
    public class CreateAccountCommand
    {
        public string Email { get; set; }

        public string? Password { get; set; } = null;

        public string UserName { get; set; }

        public bool FA2Enabled { get; set; } = true;

    }
}
