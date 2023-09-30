namespace SCP.Domain.Entity
{
    public class Rec : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Pw { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;

        public Guid SafeId { get; set; }
        public Safe Safe { get; set; }

        public ICollection<ActivityLog> ActivityLog { get; set; }
    }
}
