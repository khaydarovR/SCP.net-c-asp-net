using SCP.Domain;

namespace SCP.Domain.Entity
{
    public class Safe : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<Rec> Records { get; set; }
        public ICollection<SafeUsers> SafeUsers { get; set; }
    }
}
