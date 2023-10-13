using SCP.Application.Common;
using SCP.Application.Services;
using SCP.Domain.Entity;
using SCP.Domain;
using SCP.DAL;

namespace SCP.Application.Core.Safe
{
    public class SafeCore
    {
        private readonly CryptorService cryptorService;
        private readonly AppDbContext dbContext;

        public SafeCore(CryptorService cryptorService, AppDbContext dbContext)
        {
            this.cryptorService = cryptorService;
            this.dbContext = dbContext;
        }

        public async Task<CoreResponse<bool>> CreateUserSafe(CreateSafeCommand command)
        {
            string? encryptedKeyForSafe = string.Empty;
            if (command.ClearKey != null)
            {
                encryptedKeyForSafe = cryptorService.EncryptWithSecretKey(command.ClearKey);
            }

            var model = new Domain.Entity.Safe
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description ?? "",
                EKey = encryptedKeyForSafe,
            };
            await dbContext.Safes.AddAsync(model);

            var safeRights = SystemSafeClaims.AllClaims
                .Select(c => new SafeRight { ClaimValue = c, AppUserId = command.UserId, SafeId = model.Id });
            await dbContext.SafeRights.AddRangeAsync(safeRights);

            await dbContext.SaveChangesAsync();

            return new CoreResponse<bool>(true);
        }

        //var safes = await dbContext.Safes
        //    .Include(s => s.SafeUsers).ThenInclude(su => su.Claims)
        //    .Where(s => s.SafeUsers.Any(su => su.AppUserId == request.UserId))
        //    .Where(s => s.SafeUsers.Any(su => su.Claims.Any(c => c.ClaimValue == SystemSafeClaims.ItIsThisSafeCreator)))
        //    .ToListAsync();
    }


}
