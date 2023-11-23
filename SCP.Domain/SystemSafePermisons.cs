namespace SCP.Domain
{
    public class SystemSafePermisons
    {
        public const string GetBaseSafeInfo = "Получать информацию о сейфе";
        public const string AddRecordToSafe = "Добавлять новые записи в сейф";
        public const string GetRecordList = "Получать список записей в сейфе (без секретов)";
        public const string InviteUser = "Приглашать пользователя в сейф";
        public const string ShareWithUrl = "Делиться записью с помощью одноразовай ссылки";
        public const string KickOutUser = "Выгонять пользователя из сейфа";
        public const string EditUserRecordRight = "Менять права пользователей для конкретной записи";
        public const string EditSafe = "Редактировать сейф";
        public const string ConnectBot = "Подключать ботов";
        public const string EditUserSafeRights = "Редактировать права пользователей для сейфа";
        public const string ItIsThisSafeCreator = "Является создателем сейфа";

        public static readonly List<string> AllClaims = new List<string>()
        {
            GetBaseSafeInfo,
            AddRecordToSafe,
            GetRecordList,
            InviteUser,
            ShareWithUrl,
            KickOutUser,
            EditUserRecordRight,
            EditSafe,
            ConnectBot,
            EditUserSafeRights,
            ItIsThisSafeCreator,
        };

        public static List<string> GetReadablePermissionList(List<string> permissions)
        {
            List<string> readablePermissions = new List<string>();

            foreach (string permission in permissions)
            {
                switch (permission)
                {
                    case GetBaseSafeInfo:
                        readablePermissions.Add($"Получать информацию о сейфе.");
                        break;
                    case AddRecordToSafe:
                        readablePermissions.Add($"Добавлять новые записи в сейф.");
                        break;
                    case GetRecordList:
                        readablePermissions.Add($"Получать список записей в сейфе (без секретов).");
                        break;
                    case InviteUser:
                        readablePermissions.Add($"Приглашать пользователя в сейф.");
                        break;
                    case ShareWithUrl:
                        readablePermissions.Add($"Делиться записью с помощью одноразовой ссылки.");
                        break;
                    case KickOutUser:
                        readablePermissions.Add($"Выгонять пользователя из сейфа.");
                        break;
                    case EditUserRecordRight:
                        readablePermissions.Add($"Менять права пользователей для конкретной записи.");
                        break;
                    case EditSafe:
                        readablePermissions.Add($"Редактировать сейф.");
                        break;
                    case ConnectBot:
                        readablePermissions.Add($"Подключать ботов.");
                        break;
                    case EditUserSafeRights:
                        readablePermissions.Add($"Редактировать права пользователей для сейфа.");
                        break;
                    case ItIsThisSafeCreator:
                        readablePermissions.Add($"Является создателем сейфа.");
                        break;
                    default:
                        throw new Exception(permission + " - не найден вариант для чтения");
                        readablePermissions.Add(permission); // Если разрешение не определено, оставляем его без изменений
                        break;
                }
            }

            return readablePermissions;
        }
    }
}
