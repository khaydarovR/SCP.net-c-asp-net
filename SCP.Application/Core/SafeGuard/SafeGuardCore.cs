using SCP.Application.Common.Helpers;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Enum;

namespace SCP.Application.Core.ApiKey
{
    public class SafeGuardCore : BaseCore
    {

        private readonly AppDbContext dbContext;
        private readonly RLogService rLog;

        public SafeGuardCore(AppDbContext dbContext, RLogService rLog)
        {
            this.dbContext = dbContext;
            this.rLog = rLog;
        }


        /// <summary>
        /// Проверка разрешения пользователя для сейфа
        /// </summary>
        /// <param name="safeId"></param>
        /// <param name="userId"></param>
        /// <param name="permisionSlug"></param>
        /// <returns></returns>
        public bool AuthorHasAccessToSafe(Guid safeId, Guid userId, string permisionSlug)
        {
            var validPermisionIsExists = dbContext.SafeRights
                .Where(sr => sr.AppUserId == userId)
                .Where(sr => sr.SafeId == safeId)
                .Any(sr => sr.Permission == permisionSlug &&
                (sr.DeadDate > DateTime.UtcNow || sr.DeadDate == null));

            return validPermisionIsExists;
        }

        /// <summary>
        /// Проверка разрешения пользователя для сейфа
        /// </summary>
        /// <param name="safeId"></param>
        /// <param name="userId"></param>
        /// <param name="permisionSlug"></param>
        /// <returns></returns>
        public bool AuthorHasAccessToSafe(string safeId, Guid userId, string permisionSlug)
        {
            var validPermisionIsExists = dbContext.SafeRights
                .Where(sr => sr.AppUserId == userId)
                .Where(sr => sr.SafeId == Guid.Parse(safeId))
                .Any(sr => sr.Permission == permisionSlug &&
                (sr.DeadDate > DateTime.UtcNow || sr.DeadDate == null));

            return validPermisionIsExists;
        }

        /// <summary>
        /// Проверка разрешения пользователя для секрета
        /// </summary>
        /// <param name="usesId"></param>
        /// <param name="recId"></param>
        /// <param name="recRight"></param>
        /// <returns></returns>
        public bool IsHaveRight(Guid usesId, Guid recId, RecRightEnum recRight)
        {
            var res = dbContext.RecordRights
                .Where(rr => rr.AppUserId == usesId)
                .Where(rr => rr.RecordId == recId)
                .Any(rr => (int)rr.EnumPermission >= (int)recRight);

            if (res == false)
            {
                rLog.Push("Не успешная попытка чтения записи на основании прав: " + EnumUtil.MapRightEnumToString(recRight), recId).GetAwaiter();
            }

            return res;
        }

        /// <summary>
        /// Проверка валидности api ключа
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="safeId"></param>
        /// <returns></returns>
        public bool ApiKeyIsValid(string apiKey, Guid safeId, out string msg)
        {
            msg = "";
            var res = true;

            var k = dbContext.ApiKeys
                .Where(k => k.SafeId == safeId)
                .Where(k => k.Key == apiKey)
                .FirstOrDefault();

            if (k == null)
            {
                msg = "Ключ не найден";
                res = false;
                return res;
            }

            if (k.DeadDate < DateTime.UtcNow)
            {
                msg = "Время жизни ключа истек";
                res = false;
                return res;
            }
            if (k.IsBlocked)
            {
                msg = "Ключ временно заблокирован";
                res = false;
                return res;
            }

            return res;
        }

    }
}
