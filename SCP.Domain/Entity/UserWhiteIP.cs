namespace SCP.Domain.Entity
{
    public class UserWhiteIP : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string AllowFrom { get; set; }
    }
}
