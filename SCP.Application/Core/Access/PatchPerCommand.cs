namespace SCP.Application.Core.Access
{
    public class PatchPerCommand
    {
        public Guid AuthorId { get; set; }

        public string UserId { get; set; }
        public string SafeId { get; set; }
        public List<string> PermissionSlags { get; set; }
        public int DayLife { get; set; }
    }
}
