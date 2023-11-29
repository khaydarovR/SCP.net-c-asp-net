using SCP.Application.Common;
using SCP.Application.Common.Exceptions;
using SCP.Domain.Entity;
using SCP.Domain;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Services;
using SCP.Application.Common.Response;
using SCP.Application.Core.Safe;

namespace SCP.Application.Core.UserAuth
{
    public class UserAuthCore : BaseCore
    {
        private readonly UserManager<AppUser> userManager;
        private readonly JwtService jwtService;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly SafeCore safeCore;

        public UserAuthCore(UserManager<AppUser> userManager, JwtService jwtService, TwoFactorAuthService twoFactorAuthService, SafeCore safeCore)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
            this.twoFactorAuthService = twoFactorAuthService;
            this.safeCore = safeCore;
        }

        public async Task<CoreResponse<bool>> Activate2FA(string uId, bool isOn)
        {
            var u =  await userManager.FindByIdAsync(uId);
            await userManager.SetTwoFactorEnabledAsync(u, isOn);
            return Good<bool>(true);
        }

        public async Task<CoreResponse<bool>> CreateAccount(CreateAccountCommand command)
        {
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

            var result = await userManager.CreateAsync(model, command.Password);
            if (result.Succeeded)
            {
                var dbUser = await userManager.FindByEmailAsync(command.Email);
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Role, SystemRoles.User)
                };

                var claimRes = await userManager.AddClaimsAsync(dbUser, claims);
                if (claimRes.Succeeded)
                {
                    _ = await safeCore.CreateUserSafe(new CreateSafeCommand
                    {
                        Title = dbUser.NormalizedUserName!,
                        Description = "Сейф по умолчанию",
                        UserId = dbUser.Id,
                    });
;                   return new CoreResponse<bool>(true);
                }

                return new CoreResponse<bool>(claimRes.Errors.Select(e => e.Description));
            }
            return new CoreResponse<bool>(result.Errors.Select(e => e.Description + ". Использовать латинские буквы!"));
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
            await twoFactorAuthService.SendTwoFactorCodeByEmailAsync(user);
            return Good(true);
        }
    }
}
