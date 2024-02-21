using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.Domain.Entity;

namespace SCP.Application.Core.UserInf
{
    public class UserCore : BaseCore
    {
        private readonly UserManager<AppUser> userManager;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly SafeCore safeCore;
        private readonly AccessCore accessCore;
        private readonly UserService userService;
        private readonly CacheService cache;
        private readonly JwtService jwt;

        public UserCore(UserManager<AppUser> userManager,
                            TwoFactorAuthService twoFactorAuthService,
                            SafeCore safeCore,
                            CacheService cache,
                            AccessCore accessCore,
                            UserService userService,
                            JwtService jwt, ILogger<ApiKeyCore> logger) : base(logger)
        {
            this.userManager = userManager;
            this.twoFactorAuthService = twoFactorAuthService;
            this.safeCore = safeCore;
            this.cache = cache;
            this.accessCore = accessCore;
            this.userService = userService;
            this.jwt = jwt;
        }

        public async Task<CoreResponse<UserInfoResponse>> GetUserInfo(string userId)
        {
            var u = await userManager.FindByIdAsync(userId);

            if (u == null)
            {
                return Bad<UserInfoResponse>($"User with id:{userId} not found");
            }

            var result = new UserInfoResponse();

            result.UserId = userId;
            result.UserName = u.UserName;
            result.UserEmail = u.Email;
            result.EmailVerified = u.EmailConfirmed;
            result.FA2Enabled = u.TwoFactorEnabled;
            result.UserRole = await userService.GetRoleFromClaims(u);
            result.Jwt = await jwt.GenerateJwtToken(u);

            return Good(result);
        }



    }
}
