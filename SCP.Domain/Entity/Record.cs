namespace SCP.Domain.Entity
{
    public class Record : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string ELogin { get; set; } = null!;
        public string EPw { get; set; } = null!;
        public string ESecret { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;

        public Guid SafeId { get; set; }
        public Safe Safe { get; set; }

        public ICollection<ActivityLog> ActivityLog { get; set; }
        public ICollection<RecordRight> UserRights { get; set; }
    }
}
