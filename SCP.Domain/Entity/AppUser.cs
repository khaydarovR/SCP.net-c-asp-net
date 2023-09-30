
using Microsoft.AspNetCore.Identity;

namespace SCP.Domain.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public IList<SafeUsers> SafeUsers { get; set; }
        public IList<RecUsers> RecUsers { get; set; }
        public ICollection<ActivityLog> ChangerHistory { get; set; }
        public ICollection<WhiteIPList> WhiteIPs { get; set; }
    }
}
