namespace SCP.Domain
{
    public struct Permision
    {
        public string Slug { get; init; }
        public string Name { get; init; }
    }
    public class SystemSafePermisons
    {
        public static Permision GetBaseSafeInfo = new Permision { Slug = "Получать информацию о сейфе", Name = "Получать информацию о сейфе" };
        public static Permision GetRecordList = new Permision { Slug = "Получать список секретов в сейфе (без секретов)", Name = "Получать список секретов в сейфе (без чтения)" };
        public static Permision ReadSecrets = new Permision { Slug = "Читать секреты в сейфе!", Name = "Читать секреты в сейфе" };
        public static Permision AddRecordToSafe = new Permision { Slug = "Добавлять новые секреты в сейф", Name = "Добавлять новые секреты в сейф" };
        public static Permision ReadAndEditSecrets = new Permision { Slug = "Редактировать секреты!", Name = "Редактировать секреты" };
        public static Permision SoftDelete = new Permision { Slug = "Удалять секреты!", Name = "Удалять секреты" };
        public static Permision InviteUser = new Permision { Slug = "Приглашать пользователя в сейф", Name = "Приглашать пользователя в сейф" };
        public static Permision ShareWithUrl = new Permision { Slug = "Делиться записью с помощью одноразовай ссылки", Name = "Делиться записью с помощью одноразовай ссылки" };
        public static Permision KickOutUser = new Permision { Slug = "Выгонять пользователя из сейфа", Name = "Выгонять пользователя из сейфа" };
        public static Permision EditUserRecordRight = new Permision { Slug = "Менять права пользователей для конкретной записи", Name = "Менять права пользователей для конкретной записи" };
        public static Permision EditSafe = new Permision { Slug = "Редактировать сейф", Name = "Редактировать сейф" };
        public static Permision ApiKeyGen = new Permision { Slug = "Подключать ботов. Создовать токены доступа", Name = "Создовать API ключи для сейфа" };
        public static Permision ImportExcelFileToSafe = new Permision { Slug = "Импортировать из Excel", Name = "Импортировать секреты из Excel в сейф" };
        public static Permision ExportRecordsToExcel = new Permision { Slug = "Экспортировать секреты в Excel", Name = "Экспортировать секреты из сейфа в Excel" };
        public static Permision EditUserSafeRights = new Permision { Slug = "Редактировать права пользователей для сейфа", Name = "Редактировать права пользователей для сейфа" };
        public static Permision ItIsThisSafeCreator = new Permision { Slug = "Является создателем сейфа", Name = "Является создателем сейфа" };

        public static readonly IReadOnlyList<Permision> AllPermisions = new List<Permision>()
        {
            GetBaseSafeInfo,
            GetRecordList,
            ReadSecrets,
            AddRecordToSafe,
            ReadAndEditSecrets,
            SoftDelete,
            InviteUser,
            ShareWithUrl,
            KickOutUser,
            EditUserRecordRight,
            EditSafe,
            ApiKeyGen,
            ImportExcelFileToSafe,
            ExportRecordsToExcel,
            EditUserSafeRights,
            ItIsThisSafeCreator,
        };
    }
}
