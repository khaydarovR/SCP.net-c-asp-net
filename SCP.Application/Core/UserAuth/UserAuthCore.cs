using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SCP.Application.Common;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.Response;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.Safe;
using SCP.Application.Core.UserWhiteIP;
using SCP.Application.Services;
using SCP.Domain;
using SCP.Domain.Entity;
using System.Security.Claims;

namespace SCP.Application.Core.UserAuth
{
    public class UserAuthCore : BaseCore
    {
        private readonly UserManager<AppUser> userManager;
        private readonly JwtService jwtService;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly SafeCore safeCore;
        private readonly AccessCore accessCore;
        private readonly CacheService cache;
        private readonly UserWhiteIPCore userWhiteIP;

        public UserAuthCore(UserManager<AppUser> userManager,
                            JwtService jwtService,
                            TwoFactorAuthService twoFactorAuthService,
                            SafeCore safeCore,
                            CacheService cache,
                            UserWhiteIPCore userWhiteIP,
                            AccessCore accessCore, ILogger<ApiKeyCore> logger) : base(logger)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
            this.twoFactorAuthService = twoFactorAuthService;
            this.safeCore = safeCore;
            this.cache = cache;
            this.userWhiteIP = userWhiteIP;
            this.accessCore = accessCore;
        }

        public async Task<CoreResponse<bool>> Activate2FA(string uId, bool isOn)
        {
            var u = await userManager.FindByIdAsync(uId);
            _ = await userManager.SetTwoFactorEnabledAsync(u, isOn);
            return Good<bool>(true);
        }

        public async Task<CoreResponse<bool>> CreateAccount(CreateAccountCommand command)
        {
            if (command.CurrentIp == null)
            {
                return Bad<bool>("Не удалось получить ваш разрешенный IP аддресс");
            }

            if (userManager.Users.Any(u => u.Email == command.Email))
            {
                return Bad<bool>("Пользователь с email " + command.Email + " уже зарегистрирован");
            }
            var model = new AppUser
            {
                UserName = command.UserName,
                Email = command.Email,
                TwoFactorEnabled = true,
            };

            IdentityResult result = null;

            if (command.Password != null)
            {
                result = await userManager.CreateAsync(model, command.Password);
            }
            else
            {
                result = await userManager.CreateAsync(model);
            }

            if (result.Succeeded)
            {
                var dbUser = await userManager.FindByEmailAsync(command.Email);
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Role, SystemRoles.User)
                };

                var claimRes = await userManager.AddClaimAsync(dbUser, new Claim(ClaimTypes.Role, SystemRoles.User));
                if (claimRes.Succeeded)
                {
                    _ = await safeCore.CreateUserSafe(new CreateSafeCommand
                    {
                        Title = dbUser.NormalizedUserName!,
                        Description = "Сейф по умолчанию",
                        UserId = dbUser.Id,
                    });

                    _ = TryDeferredInvite(dbUser);

                    _ = await userWhiteIP.Create(dbUser.Id, command.CurrentIp);

                    return new CoreResponse<bool>(true);
                }

                return new CoreResponse<bool>(claimRes.Errors.Select(e => e.Description));
            }
            return new CoreResponse<bool>(result.Errors.Select(e => e.Description + ". Использовать латинские буквы!"));
        }

        private async Task TryDeferredInvite(AppUser dbUser)
        {
            if (cache.Exists(CachePrefix.DeferredInvite_ + dbUser.Email))
            {
                var safeId = cache.Get<string>(CachePrefix.DeferredInvite_ + dbUser.Email)!;
                _ = await accessCore.AddPermisionsToUsersForSafe(new string[] { SystemSafePermisons.GetBaseSafeInfo.Slug },
                                                       Guid.Parse(safeId),
                                                       new HashSet<string> { dbUser.Id.ToString() },
                                                       30);
            }
            cache.Delete(CachePrefix.DeferredInvite_ + dbUser.Email);
        }

        public async Task<CoreResponse<AuthResponse>> GetJwtAndClaims(GetJwtQuery query)
        {
            var user = await userManager.FindByEmailAsync(query.Email);
            if (user == null)
            {
                return Bad<AuthResponse>("Логин или пароль не верный");
            }

            var pwIsVerified = await userManager.CheckPasswordAsync(user, query.Password);
            if (!pwIsVerified)
            {
                return Bad<AuthResponse>("Логин или пароль не верный");
            }

            // Check if Two-Factor Authentication is enabled for the user
            if (await userManager.GetTwoFactorEnabledAsync(user))
            {

                // Add logic to prompt the user for the 2FA code, then verify it
                if (string.IsNullOrEmpty(query.Fac))
                {
                    return Bad<AuthResponse>("Требуется код двухфакторной аутентификации");
                }

                var isTwoFactorTokenValid = await twoFactorAuthService.VerifyTwoFactorCodeAsync(user, query.Fac);
                if (!isTwoFactorTokenValid)
                {
                    return Bad<AuthResponse>("Неверный код двухфакторной аутентификации");
                }
            }

            // At this point, user credentials and 2FA (if enabled) are verified, generate JWT token
            var token = await jwtService.GenerateJwtToken(user);

            return Good(new AuthResponse
            {
                UserId = user.Id.ToString(),
                Jwt = token,
                UserName = user.UserName!
            });
        }

        public async Task<CoreResponse<bool>> Send2FA(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (user.TwoFactorEnabled)
                {
                    await twoFactorAuthService.SendTwoFactorCodeByEmailAsync(user);
                }
            }
            return Good(true);
        }
    }
}
