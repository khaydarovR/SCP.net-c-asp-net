namespace SCP.Domain
{
    public class SystemSafeClaims
    {
        public const string GetBaseSafeInfo = "Получать информацию о сейфе";
        public const string InviteUser = "Приглашать пользователя в сейф";
        public const string KickOutUser = "Выгонять пользователя из сейфа";
        public const string EditUserRecordRight = "Менять права пользователей для конкретной записи";
        public const string EditSafe = "Редактировать сейф, в том числе его ключ";
        public const string ConnectBot = "Подключать ботов";
        public const string EditUserSafeClaims = "Редактировать права пользователей для сейфа";
        public const string ItIsThisSafeCreator = "Является создателем сейфа";

        public static readonly List<string> AllClaims = new List<string>()
        {
            GetBaseSafeInfo,
            InviteUser,
            KickOutUser,
            EditUserRecordRight,
            EditSafe,
            ConnectBot,
            EditUserSafeClaims,
            ItIsThisSafeCreator,
        };
    }
}
