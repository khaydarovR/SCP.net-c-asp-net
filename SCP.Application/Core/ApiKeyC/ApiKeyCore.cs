using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Core.ApiKey;
using SCP.DAL;
using SCP.Domain;
using System.Security.Cryptography;

namespace SCP.Application.Core.ApiKeyC
{
    public class ApiKeyCore : BaseCore
    {

        private readonly AppDbContext dbContext;
        private readonly SafeGuardCore safeGuard;

        public ApiKeyCore(AppDbContext dbContext, SafeGuardCore safeGuard, ILogger<ApiKeyCore> logger): base(logger)
        {
            this.dbContext = dbContext;
            this.safeGuard = safeGuard;
        }



        public async Task<CoreResponse<bool>> CreateApiKey(CreateKeyCommand cmd)
        {

            var canGenKey = safeGuard.AuthorHasAccessToSafe(cmd.SafeId, cmd.UserId, SystemSafePermisons.ApiKeyGen.Slug);
            if (canGenKey == false)
            {
                return Bad<bool>("Отсутсвует права на создание ключа доступа к сейфу: " + SystemSafePermisons.ApiKeyGen.Name);
            }
            var model = new Domain.Entity.ApiKey
            {
                DeadDate = DateTime.UtcNow.AddDays(cmd.DayLife),
                Key = GenerateApiKey(),
                Name = cmd.Name,
                OwnerId = cmd.UserId,
                SafeId = Guid.Parse(cmd.SafeId),
            };

            dbContext.ApiKeys.Add(model);
            dbContext.SaveChanges();
            return Good(true);

        }

        public async Task<CoreResponse<List<ApiKeyResponse>>> GetKeys(Guid authorId)
        {
            var res = dbContext.ApiKeys
                .Where(k => k.OwnerId == authorId)
                .Include(k => k.Safe)
                .ToList();

            var vm = new List<ApiKeyResponse>();
            foreach (var key in res)
            {
                var item = key.Adapt<ApiKeyResponse>();
                item.SafeName = key.Safe.Title;
                vm.Add(item);
            }

            return Good(vm);

        }


        public string GenerateApiKey()
        {
            using var provider = new RNGCryptoServiceProvider();
            var bytes = new byte[32];
            provider.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        public async Task<CoreResponse<bool>> Delete(string keyId)
        {
            var m = await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == Guid.Parse(keyId)) ?? null;
            if (m == null)
            {
                return Good(false);
            }
            dbContext.ApiKeys.Remove(m);
            dbContext.SaveChanges();

            return Good(true);
        }


        public async Task<CoreResponse<bool>> Block(string keyId, bool isBlock)
        {
            var k = await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == Guid.Parse(keyId));
            k.IsBlocked = isBlock;
            dbContext.ApiKeys.Update(k);
            dbContext.SaveChanges();
            return Good<bool>(true);
        }
    }
}