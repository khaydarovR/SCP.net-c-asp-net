namespace SCP.Application.Common.Response
{
    public class SafeStatResponse
    {
        public int SafeUsersAmount { get; set; }
        public int UsersCanReadSec { get; set; }
        public int UsersCanEditPer { get; set; }
        public int SecretsAmount { get; set; }
    }
}
