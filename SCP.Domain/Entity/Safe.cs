namespace SCP.Domain.Entity
{
    public class Safe : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string EPrivateKpem { get; set; } = null!;
        public string PublicKpem { get; set; } = null!;
        public virtual IList<Record> Records { get; set; }
        public virtual IList<SafeRight> SafeUsers { get; set; }
        public virtual IList<ApiKey> ApiKeys { get; set; }
    }
}
