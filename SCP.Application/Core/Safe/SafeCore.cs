using SCP.Application.Common;
using SCP.Application.Services;
using SCP.Domain.Entity;
using SCP.Domain;
using SCP.DAL;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Core.UserAuth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SCP.Application.Common.Response;
using Mapster;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using SCP.Application.Core.ApiKey;

namespace SCP.Application.Core.Safe
{
    public class SafeCore: BaseCore
    {
        private readonly SymmetricCryptoService cryptorService;
        private readonly AppDbContext dbContext;
        private readonly AsymmetricCryptoService asymmetricCrypto;
        private readonly SymmetricCryptoService symmetricCrypto;
        private readonly SafeGuardCore safeGuard;

        public SafeCore(SymmetricCryptoService cryptorService,
                        AppDbContext dbContext,
                        AsymmetricCryptoService asymmetricCrypto,
                        SymmetricCryptoService symmetricCrypto,
                        SafeGuardCore safeGuard)
        {
            this.cryptorService = cryptorService;
            this.dbContext = dbContext;
            this.asymmetricCrypto = asymmetricCrypto;
            this.symmetricCrypto = symmetricCrypto;
            this.safeGuard = safeGuard;
        }

        public async Task<CoreResponse<bool>> CreateUserSafe(CreateSafeCommand command)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return Bad<bool>("Название сейфа не может быть пустым");
            }

            var keys = asymmetricCrypto.GenerateKeys();
            Console.WriteLine("Public Key: " + keys.publicKeyPem);
            Console.WriteLine("Private Key: " + keys.privateKeyPem);

            string PublickKeyPem = keys.publicKeyPem;
            string PrivateKeyPem = keys.privateKeyPem;


            // Encrypt private key before saving
            var EPrivateKeyPkcs8 = cryptorService.EncryptWithSecretKey(PrivateKeyPem);
            var sharedId = Guid.NewGuid();

            var permisions = SystemSafePermisons.AllPermisions
                .Select(c => new SafeRight
                {
                    AppUserId = command.UserId,
                    Permission = c.Slug,
                    DeadDate = null
                })
                .ToList();

            var model = new Domain.Entity.Safe
            {
                Id = sharedId,
                Title = command.Title,
                Description = command.Description ?? "",
                PublicKpem = PublickKeyPem,
                EPrivateKpem = EPrivateKeyPkcs8,
                SafeUsers = permisions
            };


            await dbContext.Safes.AddAsync(model);
            await dbContext.SafeRights.AddRangeAsync(permisions);

            await dbContext.SaveChangesAsync();

            return new CoreResponse<bool>(true);
        }


        /// <summary>
        /// Получение всех сейфов связанных с пользователем
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<CoreResponse<List<GetLinkedSafeResponse>>> GetLinked(GetLinkedSafesQuery query)
        {
            var safes = await dbContext.Safes
                .Include(s => s.SafeUsers)
                .Where(s => s.SafeUsers.Any(su => su.AppUserId == query.UserId))
                .ProjectToType<GetLinkedSafeResponse>()
                .ToListAsync();


            return Good(safes);
        }


        /// <summary>
        /// Получить всех пользователей с правами для сейфа 
        /// </summary>
        /// <param name="safeId"></param>
        /// <returns></returns>
        public async Task<CoreResponse<List<AppUser>>> GetAllUsersFromSafe(Guid safeId)
        {
            var users = dbContext.AppUsers
                .Include(u => u.SafeRights)
                .Where(u => u.SafeRights.Any(u => u.SafeId == safeId))
                .ToList();
            return Good<List<AppUser>>(users);
        }

        public CoreResponse<string> GetPubKForSafe(string safeId)
        {
            var safePubK = dbContext.Safes
                .Where(s => s.Id == Guid.Parse(safeId))
                .Select(s => s.PublicKpem)
                .FirstOrDefault();

            if (safePubK == null)
            {
                return new CoreResponse<string>("Сейф не найден", false);
            }

            return Good(safePubK);
        }


        public string GetClearPrivateKeyFromSafe(string safeId)
        {
            var ePrivateKey = dbContext.Safes
                .Where(s => s.Id == Guid.Parse(safeId))
                .Select(s => s.EPrivateKpem)
                .FirstOrDefault();

            var privateKey = symmetricCrypto.DecryptWithSecretKey(ePrivateKey);

            return privateKey;

        }


        public async Task<CoreResponse<SafeStatResponse>> GetStat(Guid safeId)
        {
            var usersFromSafe = await GetAllUsersFromSafe(safeId);
            
            var canReadCounter = 0;
            var canEditPerCounter = 0;
            foreach (var user in usersFromSafe.Data)
            {
                if (safeGuard.AuthorHasAccessToSafe(safeId, user.Id, SystemSafePermisons.ReadSecrets.Slug))
                {
                    canReadCounter++;
                }
                if (safeGuard.AuthorHasAccessToSafe(safeId, user.Id, SystemSafePermisons.EditUserSafeRights.Slug))
                {
                    canEditPerCounter++;
                }
                if (safeGuard.AuthorHasAccessToSafe(safeId, user.Id, SystemSafePermisons.EditUserSafeRights.Slug))
                {
                    canEditPerCounter++;
                }
            }


            var result = new SafeStatResponse()
            {
                UsersCanEditPer = canEditPerCounter,
                UsersCanReadSec = canReadCounter,
                SafeUsersAmount = usersFromSafe.Data.Count,
                SecretsAmount = dbContext.Records.Where(r => r.SafeId == safeId).Count(),
            };

            return Good(result);
        }
    }

}
