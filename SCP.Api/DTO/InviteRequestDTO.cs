namespace SCP.Api.DTO
{
    public class InviteRequestDTO
    {
        public List<string> SafeIds { get; set; }
        public List<string> UserIds { get; set; }
        public List<string> UserEmails { get; set; }
        public List<string> Permisions { get; set; }
        public int DayLife { get; set; }
    }
}
