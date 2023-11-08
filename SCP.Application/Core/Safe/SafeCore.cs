using SCP.Application.Common;
using SCP.Application.Services;
using SCP.Domain.Entity;
using SCP.Domain;
using SCP.DAL;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Core.UserAuth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace SCP.Application.Core.Safe
{
    public class SafeCore
    {
        private readonly SymmetricCryptoService cryptorService;
        private readonly AppDbContext dbContext;

        public SafeCore(SymmetricCryptoService cryptorService, AppDbContext dbContext)
        {
            this.cryptorService = cryptorService;
            this.dbContext = dbContext;
        }

        public async Task<CoreResponse<bool>> CreateUserSafe(CreateSafeCommand command)
        {
            var rsa = new RSACryptoServiceProvider(2048);
                                                          
            string publicKey = rsa.ToXmlString(false);
            string privateKey = rsa.ToXmlString(true);

            //string? encryptedKeyForSafe = string.Empty;
            //if (command.ClearKey != null)
            //{
            //    encryptedKeyForSafe = cryptorService.EncryptWithSecretKey(command.ClearKey);
            //}

            var model = new Domain.Entity.Safe
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description ?? "",
                PublicK = publicKey,
                PrivateK = privateKey,
            };
            await dbContext.Safes.AddAsync(model);

            var cliamValues = SystemSafeClaims.AllClaims
                .Select(c => new SafeRight { AppUserId = command.UserId, SafeId = model.Id, ClaimValue = c })
                .ToList();

            await dbContext.SafeRights.AddRangeAsync(cliamValues);

            await dbContext.SaveChangesAsync();

            return new CoreResponse<bool>(true);
        }

        public async Task<CoreResponse<List<Domain.Entity.Safe>>> GetLinkedSafes(GetLinkedSafesQuery query)
        {
            var safes = await dbContext.Safes
                .Include(s => s.SafeUsers)
                .Where(s => s.SafeUsers.Any(su => su.AppUserId == query.UserId))
                .Where(s => s.SafeUsers.Any(su => su.ClaimValue == SystemSafeClaims.ItIsThisSafeCreator))
                .ToListAsync();

            return new CoreResponse<List<Domain.Entity.Safe>>(safes);
        }
    }


}
