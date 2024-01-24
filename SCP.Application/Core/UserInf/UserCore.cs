using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SCP.Api.DTO;
using SCP.Application.Common;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.Domain.Entity;

namespace SCP.Application.Core.UserAuth
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

        public async Task<CoreResponse<GoogleUserInfoResponse>> GetUserInfo(string userId)
        {
            var u = await userManager.FindByIdAsync(userId);

            if (u == null)
            {
                return Bad<GoogleUserInfoResponse>("UserId is not available");
            }

            var result = new GoogleUserInfoResponse();

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
