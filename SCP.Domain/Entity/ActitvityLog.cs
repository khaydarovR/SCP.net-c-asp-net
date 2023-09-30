namespace SCP.Domain.Entity
{
    public class ActivityLog : BaseEntity
    {
        public DateTime At { get; set; }
        public string Text { get; set; }

        public Guid RecordId { get; set; }
        public Rec Record { get; set; }

        public Guid AppUsreId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
