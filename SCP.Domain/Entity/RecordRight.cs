using SCP.Domain.Enum;
using SCP.Domain.Entity;

namespace SCP.Domain.Entity
{
    public class RecordRight
    {
        public Guid AppUserId { get; set; }
        public Guid RecordId { get; set; }
        public RecRightEnum EnumPermission { get; set; }
    }
}
