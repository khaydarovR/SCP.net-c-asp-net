using FluentValidation.Results;
using SCP.Application.Common;
using SCP.Application.Common.Validators;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;

namespace SCP.Application.Core.Access
{
    public class AccessCore: BaseCore
    {
        private AppDbContext dbContext { get; set; }

        public AccessCore(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public async Task<CoreResponse<string>> AuthorizeUsersToSafes(AuthrizeUsersToSafeCommand cmd)
        {
            InviteUsersToSafesV validator = new();
            ValidationResult results = validator.Validate(cmd);
            if (results.IsValid == false)
            {
                return Bad<string>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            //проверить разрешения автора на сейфах
            foreach (var safeId in cmd.SafeIds)
            {
                if(UserHasAccess(Guid.Parse(safeId), cmd.AuthorId, SystemSafePermisons.InviteUser) == false)
                {
                    return Bad<string>($"В сейфе {cmd.SafeIds} отсутсвует разрешение на: " + SystemSafePermisons.InviteUser) ;
                }
            }

            //добавление разрешений для каждого сейфа
            foreach (var safeId in cmd.SafeIds)
            {
                _ = await AddPermisionsToUsersForSafe(cmd.Permisions.ToArray(),
                                                     Guid.Parse(safeId),
                                                     cmd.UserIds.Select(i => Guid.Parse(i)).ToArray(),
                                                     cmd.DayLife);
            }


            return Good<string>($"К {cmd.SafeIds.Count} сейфам доблены {cmd.UserIds.Count} пользователя");

        }

        private async Task<bool> AddPermisionsToUsersForSafe(string[] permisions, Guid safeId, Guid[] userIds, int lifeOfTime)
        {
            foreach (var per in permisions)
            {
                foreach(var uId in userIds)
                {
                    dbContext.SafeRights.Add(new SafeRight
                    {
                        SafeId = safeId,
                        AppUserId = uId,
                        Permission = per,
                        DeadDate = DateTime.UtcNow.AddDays(lifeOfTime)
                    });
                }
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        private bool UserHasAccess(Guid safeId, Guid userId, string permision)
        {
            var validPermisionIsExists = dbContext.SafeRights
                .Where(sr => sr.AppUserId == userId)
                .Where(sr => sr.SafeId == safeId)
                .Any(sr => sr.Permission == permision && sr.DeadDate < DateTime.UtcNow);

            return validPermisionIsExists;
        }
    }
}
