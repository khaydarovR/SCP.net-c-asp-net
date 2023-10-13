using SCP.Domain;

namespace SCP.Domain.Entity
{
    public class Safe : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string EKey { get; set; } = null!;
        public ICollection<Record> Records { get; set; }
        public ICollection<SafeRight> SafeUsers { get; set; }
    }
}
