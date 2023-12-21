using SCP.Domain.Enum;

namespace SCP.Application.Common.Helpers
{
    public static class EnumUtil
    {
        public static string MapRightEnumToString(RecRightEnum enumPermission)
        {
            switch (enumPermission)
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
                    throw new ArgumentOutOfRangeException(nameof(enumPermission), enumPermission, "The provided argument doesn't match any cases in the enum RecRightEnum");
            }
        }
    }
}
