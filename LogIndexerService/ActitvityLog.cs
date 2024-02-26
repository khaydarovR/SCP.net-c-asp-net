namespace SCP.Domain.Entity
{
    public class ActivityLog : BaseEntity
    {
        public DateTime At { get; set; }
        public string LogText { get; set; }

        public Guid RecordId { get; set; }
        public Record Record { get; set; }
    }
}
