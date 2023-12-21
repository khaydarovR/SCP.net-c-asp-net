using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Common;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.Response;
using SCP.Application.Common.Validators;
using SCP.Application.Core.ApiKey;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;

namespace SCP.Application.Core.Access
{
    public class AccessCore : BaseCore
    {
        private readonly SafeCore safeCore;
        private readonly SafeGuardCore safeGuard;
        private readonly CacheService cache;
        private readonly AppDbContext dbContext;
        private readonly UserManager<AppUser> userMan;

        public AccessCore(AppDbContext dbContext,
                          SafeCore safeCore,
                          SafeGuardCore safeGuard,
                          CacheService cache,
                          UserManager<AppUser> userMan)
        {
            this.dbContext = dbContext;
            this.safeCore = safeCore;
            this.safeGuard = safeGuard;
            this.cache = cache;
            this.userMan = userMan;
        }



        public async Task<CoreResponse<string>> AuthorizeUsersToSafes(AuthrizeUsersToSafeCommand cmd)
        {

            InviteUsersToSafesV validator = new();
            ValidationResult results = validator.Validate(cmd);
            if (results.IsValid == false)
            {
                return Bad<string>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            //проверка всех разрешений
            foreach (var safeId in cmd.SafeIds)
            {
                var hasAccess = safeGuard.AuthorHasAccessToSafe(Guid.Parse(safeId), cmd.AuthorId, SystemSafePermisons.InviteUser.Slug);
                if (hasAccess == false)
                {
                    return Bad<string>($"В сейфе отсутсвует разрешение на: " + SystemSafePermisons.InviteUser.Name);
                }

                var hasEditPer = safeGuard.AuthorHasAccessToSafe(safeId, cmd.AuthorId, SystemSafePermisons.EditUserSafeRights.Slug);
                if (hasEditPer == false)
                {
                    if (cmd.Permisions.Count != 1 || !cmd.Permisions.Contains(SystemSafePermisons.GetBaseSafeInfo.Slug))
                    {
                        return Bad<string>("Вам разрешено только приглашать пользователей в сейф с помощью: "
                                + SystemSafePermisons.GetBaseSafeInfo.Name);
                    }
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

        public async Task<bool> AddPermisionsToUsersForSafe(string[] permisionSlugs, Guid safeId, HashSet<string> userIds, int lifeOfTime)
        {
            foreach (var uId in userIds)
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
        /// Поиск связанных пользователей с текущим пользователем (общие сейфы)
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
                .Where(u => u.Id != currentUserId)
                .Where(u => linkedUserIds.Contains(u.Id))
                .ProjectToType<GetUserResponse>()
                .ToListAsync();

            return Good<List<GetUserResponse>>(linkedUsers);

        }


        /// <summary>
        /// Список разрешений для пользователя в сейфе
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public CoreResponse<Permision[]> GetPermissions(GetPerQuery cmd)
        {
            var perSlugs = dbContext.SafeRights
                .Where(sr => sr.SafeId == cmd.SafeId)
                .Where(sr => sr.AppUserId == cmd.UserId)
                .Select(sr => sr.Permission)
                .ToArray();

            var res = SystemSafePermisons.AllPermisions
                .Where(p => perSlugs.Any(ps => ps == p.Slug))
                .ToArray();

            return Good<Permision[]>(res);
        }


        /// <summary>
        /// Обновить разрешения для пользвателя в сейфе
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public async Task<CoreResponse<bool>> UpdatePermissions(PatchPerCommand cmd)
        {
            PatchPerV validator = new();
            ValidationResult results = validator.Validate(cmd);
            if (results.IsValid == false)
            {
                return Bad<bool>(results.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var hasEditPer = safeGuard.AuthorHasAccessToSafe(cmd.SafeId, cmd.AuthorId, SystemSafePermisons.EditUserSafeRights.Slug);
            if (hasEditPer == false)
            {
                return Bad<bool>("Отсутствует разрешение: "
                    + SystemSafePermisons.EditUserSafeRights.Name);
            }

            ClearPermisionsInSafe(Guid.Parse(cmd.SafeId), cmd.UserId);

            var users = new HashSet<string> { cmd.UserId };
            var res = await AddPermisionsToUsersForSafe(cmd.PermissionSlags.ToArray(), Guid.Parse(cmd.SafeId), users, cmd.DayLife);
            return Good<bool>(res);
        }


        public async Task<CoreResponse<string>> JustInvite(string safeId, string email, Guid authorId)
        {
            var hasAccess = safeGuard.AuthorHasAccessToSafe(safeId, authorId, SystemSafePermisons.InviteUser.Slug);
            if (hasAccess == false)
            {
                return Bad<string>($"В сейфе отсутсвует разрешение на: " + SystemSafePermisons.InviteUser.Name);
            }

            var user = await userMan.FindByEmailAsync(email);

            if (user == null)
            {
                await cache.Save(CachePrefix.DeferredInvite_ + email, safeId, 48 * 60);
                return Good(email + " должен зарегистрироваться в системе в течении 2 суток для получения досутпа к сейфу");
            }

            if (user.Id == authorId)
            {
                return Bad<string>($"Запрещено редактировать свои права");
            }

            var inviteResult = await AddPermisionsToUsersForSafe(new string[] { SystemSafePermisons.GetBaseSafeInfo.Slug },
                                                                 Guid.Parse(safeId),
                                                                 new HashSet<string> { user!.Id.ToString() },
                                                                 30);

            if (inviteResult == false)
            {
                return Bad<string>("Ошибка при добавлении разрешения");
            }

            return Good(email + " успешно добавлен в сейф согласно базовой политике");
        }
    }
}
