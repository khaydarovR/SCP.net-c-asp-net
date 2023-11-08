using SCP.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCP.Domain.Entity
{
    public class SafeRight
    {
        public Guid AppUserId { get; set; }
        public Guid SafeId { get; set; }
        
        public string ClaimValue { get; set; }
        public DateTime DeadDate{ get; set; }
    }
}
