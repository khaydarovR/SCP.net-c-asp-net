namespace SCP.Domain.Entity
{
    public class Record : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string ELogin { get; set; } = null!;
        public string EPw { get; set; } = null!;
        public string ESecret { get; set; } = null!;
        public string ForResource { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;

        public Guid SafeId { get; set; }
        public Safe Safe { get; set; }

        public virtual IList<ActivityLog> ActivityLogs { get; set; }
        public virtual IList<RecordRight> RightUsers { get; set; }
    }
}
