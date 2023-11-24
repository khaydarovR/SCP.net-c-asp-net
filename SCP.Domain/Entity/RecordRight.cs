using SCP.Domain.Enum;
using SCP.Domain.Entity;

namespace SCP.Domain.Entity
{
    public class RecordRight: BaseEntity
    {
        public Guid AppUserId { get; set; }
        public Guid RecordId { get; set; }
        public RecRightEnum EnumPermission { get; set; }

        public virtual string MapRightEnumToString()
        {
            switch (EnumPermission)
            {
                case RecRightEnum.See:
                    return "Смотреть";
                case RecRightEnum.Read:
                    return "+Читать";
                case RecRightEnum.Edit:
                    return "+Редактироват";
                case RecRightEnum.Delete:
                    return "+Удалять";
                default:
                    throw new ArgumentOutOfRangeException(nameof(EnumPermission), EnumPermission, "The provided argument doesn't match any cases in the enum RecRightEnum");
            }
        }
    }
}
