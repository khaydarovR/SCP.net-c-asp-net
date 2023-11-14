
using Microsoft.AspNetCore.Identity;

namespace SCP.Domain.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public virtual IList<SafeRight> SafeRights { get; set; }
        public virtual IList<RecordRight> RecordRights { get; set; }
        public virtual IList<UserWhiteIP> WhiteIPs { get; set; }
        public virtual IList<Bot> Bots { get; set; }
    }
}
