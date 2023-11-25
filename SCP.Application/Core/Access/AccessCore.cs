using FluentValidation.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.Safe;
using SCP.Application.Core.SafeGuard;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;
using SCP.Domain.Enum;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SCP.Application.Core.Access
{
    public class AccessCore: BaseCore
    {
        private readonly SafeCore safeCore;
        private readonly SafeGuardCore safeGuard;
        private readonly AppDbContext dbContext;

        public AccessCore(AppDbContext dbContext, SafeCore safeCore, SafeGuardCore safeGuard)
        {
            this.dbContext = dbContext;
            this.safeCore = safeCore;
            this.safeGuard = safeGuard;
        }



        public async Task<CoreResponse<string>> AuthorizeUsersToSafes(AuthrizeUsersToSafeCommand cmd)
        {

            InviteUsersToSafesV validator = new();
            ValidationResult results = validator.Validate(cmd);
            if (results.IsValid == false)
            {
                return Bad<string>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            foreach (var safeId in cmd.SafeIds)
            {
                var hasAccess = safeGuard.AuthorHasAccessToSafe(Guid.Parse(safeId), cmd.AuthorId, SystemSafePermisons.InviteUser.Slug);
                if (hasAccess == false)
                {
                    return Bad<string>($"В сейфе отсутсвует разрешение на: " + SystemSafePermisons.InviteUser.Name) ;
                }
            }

            //добавление разрешений для каждого сейфа
            foreach (var safeId in cmd.SafeIds)
            {   
                //для каждого пользователя по id
                _ = await AddPermisionsToUsersForSafe(cmd.Permisions.ToArray(),
                                                     Guid.Parse(safeId),
                                                     cmd.UserIds.ToHashSet(),
                                                     cmd.DayLife);

                //для каждого пользователя по email
                var userIds = dbContext.AppUsers
                    .Where(u => cmd.UserEmails.Any(e => e == u.Email))
                    .Select(u => u.Id.ToString()).ToHashSet();

                _ = await AddPermisionsToUsersForSafe(cmd.Permisions.ToArray(),
                                                      Guid.Parse(safeId),
                                                      userIds,
                                                      cmd.DayLife);
            }


            return Good<string>($"К {cmd.SafeIds.Count} сейфам доблены {cmd.UserIds.Count} пользователя");

        }

        private async Task<bool> AddPermisionsToUsersForSafe(string[] permisionSlugs, Guid safeId, HashSet<string> userIds, int lifeOfTime)
        {

            foreach(var uId in userIds)
            {
                if (string.IsNullOrEmpty(uId))
                {
                    continue;
                }

                ClearPermisionsInSafe(safeId, uId);

                //добавить разрешения в сейфе для пользователя
                foreach (var per in permisionSlugs)
                {
                    dbContext.SafeRights.Add(new SafeRight
                    {
                        SafeId = safeId,
                        AppUserId = Guid.Parse(uId),
                        Permission = per,
                        DeadDate = DateTime.UtcNow.AddDays(lifeOfTime)
                    });
                }

                //дать права пользователю для всех записей в сейфе
                var recordsInSafe = dbContext.Records
                    .Where(r => r.SafeId == safeId)
                    .Select(r => r.Id).ToList();

                foreach (var recordID in recordsInSafe)
                {
                    var existingRight = dbContext.RecordRights
                        .FirstOrDefault(rr => rr.AppUserId == Guid.Parse(uId) && rr.RecordId == recordID);

                    if (existingRight == null)
                    {
                        dbContext.RecordRights.Add(new RecordRight
                        {
                            AppUserId = Guid.Parse(uId),
                            RecordId = recordID,
                            EnumPermission = Domain.Enum.RecRightEnum.Delete
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        private void ClearPermisionsInSafe(Guid safeId, string uId)
        {
            var safesPermisons = dbContext.SafeRights
                .Where(s => s.SafeId == safeId)
                .Where(s => s.AppUserId == Guid.Parse(uId))
                .ToArray();

            var recIdsFromSafe = dbContext.Records
                .Where(r => r.SafeId == safeId)
                .Select(r => r.Id)
                .ToArray();
            var recRightsForUserInSafe = dbContext.RecordRights
                .Where(sr => sr.AppUserId == Guid.Parse(uId))
                .Where(sr => recIdsFromSafe.Any(id => sr.Id == id))
                .ToArray();

            dbContext.RecordRights.RemoveRange(recRightsForUserInSafe);
            dbContext.SafeRights.RemoveRange(safesPermisons);

            dbContext.SaveChanges();
        }

        public CoreResponse<Permision[]> GetSystemPermisions()
        {
            var readPermisions = SystemSafePermisons.AllPermisions.ToArray();
            return Good<Permision[]>(readPermisions);
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
