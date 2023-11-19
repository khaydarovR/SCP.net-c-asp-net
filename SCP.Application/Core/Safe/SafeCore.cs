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

namespace SCP.Application.Core.Safe
{
    public class SafeCore: BaseCore
    {
        private readonly SymmetricCryptoService cryptorService;
        private readonly AppDbContext dbContext;
        private readonly AsymmetricCryptoService asymmetricCrypto;
        private readonly SymmetricCryptoService symmetricCrypto;

        public SafeCore(SymmetricCryptoService cryptorService, AppDbContext dbContext, AsymmetricCryptoService asymmetricCrypto, SymmetricCryptoService symmetricCrypto)
        {
            this.cryptorService = cryptorService;
            this.dbContext = dbContext;
            this.asymmetricCrypto = asymmetricCrypto;
            this.symmetricCrypto = symmetricCrypto;
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
            var model = new Domain.Entity.Safe
            {
                Id = Guid.NewGuid(),
                Title = command.Title,
                Description = command.Description ?? "",
                PublicKpem = PublickKeyPem,
                EPrivateKpem = EPrivateKeyPkcs8
            };
            await dbContext.Safes.AddAsync(model);

            var permisions = SystemSafePermisons.AllClaims
                .Select(c => new SafeRight { AppUserId = command.UserId, SafeId = model.Id, Permission = c })
                .ToList();

            await dbContext.SafeRights.AddRangeAsync(permisions);

            await dbContext.SaveChangesAsync();

            return new CoreResponse<bool>(true);
        }



        public async Task<CoreResponse<List<GetLinkedSafeResponse>>> GetLinked(GetLinkedSafesQuery query)
        {
            var safes = await dbContext.Safes
                .Include(s => s.SafeUsers)
                .Where(s => s.SafeUsers.Any(su => su.AppUserId == query.UserId))
                .ProjectToType<GetLinkedSafeResponse>()
                .ToListAsync();


            return Good(safes);
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
    }


}
