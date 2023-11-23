using FluentValidation.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.Safe;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;

namespace SCP.Application.Core.Access
{
    public class AccessCore: BaseCore
    {
        private readonly SafeCore safeCore;

        private readonly AppDbContext dbContext;

        public AccessCore(AppDbContext dbContext, SafeCore safeCore)
        {
            this.dbContext = dbContext;
            this.safeCore = safeCore;
        }



        public async Task<CoreResponse<string>> AuthorizeUsersToSafes(AuthrizeUsersToSafeCommand cmd)
        {
            //обработать если только емаил -> найти и пригласить


            InviteUsersToSafesV validator = new();
            ValidationResult results = validator.Validate(cmd);
            if (results.IsValid == false)
            {
                return Bad<string>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            //проверить разрешения автора на сейфах
            //TODO: SafeGuar
            foreach (var safeId in cmd.SafeIds)
            {
                if(AuthorHasAccess(Guid.Parse(safeId), cmd.AuthorId, SystemSafePermisons.InviteUser) == false)
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

        private bool AuthorHasAccess(Guid safeId, Guid userId, string permision)
        {
            var validPermisionIsExists = dbContext.SafeRights
                .Where(sr => sr.AppUserId == userId)
                .Where(sr => sr.SafeId == safeId)
                .Any(sr => sr.Permission == permision && sr.DeadDate < DateTime.UtcNow);

            return validPermisionIsExists;
        }


        public CoreResponse<string[]> GetSystemPermisions()
        {
            var readPermisions = SystemSafePermisons.GetReadablePermissionList(SystemSafePermisons.AllClaims).ToArray();
            return Good<string[]>(readPermisions);
        }

        public Task AuthorizeUsersToSafes()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Поиск связанных пользователей с текущим пользователем
        /// currentUserId - SafeUsers - Users
        /// </summary>
        /// <param name="rootUserId"></param>
        /// <returns></returns>
        public async Task<CoreResponse<List<GetUserResponse>>> GetLinkedUsersFromSafes(Guid currentUserId)
        {
            HashSet<Guid> linkedUserIds = new HashSet<Guid>();

            // Получение всех сейфов, связанных с текущим пользователем
            var currentUserSafesResponse = await safeCore.GetLinked(new GetLinkedSafesQuery { UserId = currentUserId });

            if (!currentUserSafesResponse.IsSuccess)
            {
                return Bad<List<GetUserResponse>>(currentUserSafesResponse.ErrorList.ToArray());
            }

            // Получение всех пользователей, связанных с каждым сейфом
            foreach (var safe in currentUserSafesResponse.Data!)
            {
                var safeRights = await dbContext.SafeRights
                    .Where(sr => sr.SafeId == Guid.Parse(safe.Id))
                    .ToListAsync();

                // Добавление всех найденных пользователей в HashSet
                foreach (var safeRight in safeRights)
                {
                    linkedUserIds.Add(safeRight.AppUserId);
                }
            }

            var linkedUsers = await dbContext.AppUsers
                .Where(u => u.Id !=  currentUserId)
                .Where(u => linkedUserIds.Contains(u.Id))
                .ProjectToType<GetUserResponse>()
                .ToListAsync();

            return Good<List<GetUserResponse>>(linkedUsers);
            
        }
    }
}
