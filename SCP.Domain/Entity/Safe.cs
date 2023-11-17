using SCP.Domain;

namespace SCP.Domain.Entity
{
    public class Safe : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string EPrivateK { get; set; } = null!;
        public string PublicK { get; set; } = null!;
        public virtual IList<Record> Records { get; set; }
        public virtual IList<SafeRight> SafeUsers { get; set; }
    }
}
