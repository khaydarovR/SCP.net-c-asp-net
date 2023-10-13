
using Microsoft.AspNetCore.Identity;

namespace SCP.Domain.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public IList<SafeRight> SafeUsers { get; set; }
        public IList<RecordRight> RecUsers { get; set; }
        public ICollection<ActivityLog> ChangerHistory { get; set; }
        public ICollection<WhiteIPList> WhiteIPs { get; set; }
        public ICollection<Bot> Bots { get; set; }
    }
}
