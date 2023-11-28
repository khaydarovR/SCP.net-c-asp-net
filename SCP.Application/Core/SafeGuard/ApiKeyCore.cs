using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace SCP.Application.Core.SafeGuard
{
    public class ApiKeyCore : BaseCore
    {

        private readonly AppDbContext dbContext;

        public ApiKeyCore(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public async Task<CoreResponse<bool>> CreateApiKey(CreateKeyCommand cmd)
        {
            var model = new ApiKey
            {
                DeadDate = DateTime.UtcNow.AddDays(cmd.DayLife),
                Key = GenerateApiKey(),
                Name = cmd.Name,
                OwnerId = cmd.UserId,
                SafeId = Guid.Parse(cmd.SafeId),
            };

            dbContext.ApiKeys.Add(model);
            dbContext.SaveChanges();
            return Good<bool>(true);

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

            return Good<List<ApiKeyResponse>>(vm);

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
                return Good<bool>(false);
            }
            dbContext.ApiKeys.Remove(m);
            dbContext.SaveChanges();

            return Good<bool>(true);
        }
    }
}