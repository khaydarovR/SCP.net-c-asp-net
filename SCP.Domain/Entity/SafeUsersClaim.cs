namespace SCP.Domain.Entity
{
    public class SafeUsersClaim: BaseEntity
    {
        public Guid UserForSafeId { get; set; }
        public SafeUsers UserForSafe { get; set; }

        public string ClaimValue { get; set; }
    }
}
