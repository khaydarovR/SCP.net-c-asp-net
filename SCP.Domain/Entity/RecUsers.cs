using SCP.Domain.Enum;
using SCP.Domain.Entity;

namespace SCP.Domain.Entity
{
    public class RecUsers
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public Guid RecordId { get; set; }
        public Rec Record { get; set; }
        public RecRightEnum Right { get; set; }
    }
}
