namespace SCP.Application.Core.Record
{
    public class PatchRecordCommand
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Pw { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public string ForResource { get; set; }
        public bool IsDeleted { get; set; }

        public string Signature { get; set; } = null!;
        public string ClientPrivK { get; set; } = null!;
        public string UserId { get; set; } = null!;
    }
}
