namespace SCP.Domain
{
    public class SystemSafePermisons
    {
        public const string GetBaseSafeInfo = "Получать информацию о сейфе";
        public const string InviteUser = "Приглашать пользователя в сейф";
        public const string ShareWithUrl = "Делиться записью с помощью однаразовой ссылки";
        public const string KickOutUser = "Выгонять пользователя из сейфа";
        public const string EditUserRecordRight = "Менять права пользователей для конкретной записи";
        public const string EditSafe = "Редактировать сейф, в том числе его ключ";
        public const string ConnectBot = "Подключать ботов";
        public const string EditUserSafeRights = "Редактировать права пользователей для сейфа";
        public const string ItIsThisSafeCreator = "Является создателем сейфа";

        public static readonly List<string> AllClaims = new List<string>()
        {
            GetBaseSafeInfo,
            InviteUser,
            ShareWithUrl,
            KickOutUser,
            EditUserRecordRight,
            EditSafe,
            ConnectBot,
            EditUserSafeRights,
            ItIsThisSafeCreator,
        };
    }
}
