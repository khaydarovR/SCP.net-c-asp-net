using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Common;
using SCP.Application.Core.Access;
using SCP.Application.Core.Safe;
using SCP.DAL;
using SCP.Domain.Entity;
using SCP.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCP.Domain.Enum;

namespace SCP.Application.Core.SafeGuard
{
    public class SafeGuardCore : BaseCore
    {

        private readonly AppDbContext dbContext;

        public SafeGuardCore(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
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

            return res;
        }

    }
}
