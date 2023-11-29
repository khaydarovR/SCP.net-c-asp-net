namespace SCP.Domain.Entity
{
    public class ApiKey : BaseEntity
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public DateTime DeadDate { get; set; }
        public bool IsBlocked { get; set; }

        public Guid OwnerId {  get; set; }
        public AppUser Owner { get; set; }

        public Guid SafeId { get; set; }
        public Safe Safe {  get; set; }

        public virtual IList<ApiKeyWhiteIP> WhiteIPs { get; set; }
    }
}
