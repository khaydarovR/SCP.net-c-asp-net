using SCP.Domain.Enum;

namespace SCP.Domain.Entity
{
    public class SafeUsers
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public Guid SafeId { get; set; }
        public Safe Safe { get; set; }
        public SafeRightEnum Right { get; set; }
    }
}
