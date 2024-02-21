using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SCP.Application.Common;
using SCP.Application.Core.ApiKeyC;
using SCP.DAL;

namespace SCP.Application.Core.UserWhiteIP
{
    public class UserWhiteIPCore : BaseCore
    {

        private readonly AppDbContext dbContext;

        public UserWhiteIPCore(AppDbContext dbContext, ILogger<ApiKeyCore> logger) : base(logger)
        {
            this.dbContext = dbContext;

        }

        /// <summary>
        /// Добавление нового разрешенного IP
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newIpAddress"></param>
        /// <param name="currentIp"></param>
        /// <returns></returns>
        public async Task<CoreResponse<bool>> Create(Guid userId, string newIpAddress)
        {
            var newWhiteIp = new Domain.Entity.UserWhiteIP
            {
                AllowFrom = newIpAddress,
                AppUserId = userId,
            };

            _ = await dbContext.UserWhiteIPs.AddAsync(newWhiteIp);
            _ = await dbContext.SaveChangesAsync();
            return Good(true);
        }


        /// <summary>
        /// Удаление IP адреса из списка разрешенных
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="IpAddress"></param>
        /// <returns></returns>
        public async Task<CoreResponse<bool>> Delete(Guid userId, string ipAddress)
        {
            var ipAddressForDelete = await dbContext.UserWhiteIPs
                .Where(i => i.AllowFrom == ipAddress)
                .Where(i => i.AppUserId == userId)
                .FirstOrDefaultAsync();

            if (ipAddressForDelete != null)
            {
                _ = dbContext.UserWhiteIPs.Remove(ipAddressForDelete);
                _ = await dbContext.SaveChangesAsync();
            }

            return Good(true);
        }


        public bool IsAllowFrom(string ipAddress, Guid userId)
        {
            var isAllow = dbContext.UserWhiteIPs.Any(i => i.AllowFrom == ipAddress && i.AppUserId == userId);
            return isAllow;
        }


        public async Task<CoreResponse<List<string>>> GetAllAllowIP(Guid userId)
        {
            var result = await dbContext.UserWhiteIPs
                .Where(i => i.AppUserId == userId)
                .Select(i => i.AllowFrom)
                .ToListAsync();

            return Good(result);
        }

    }
}