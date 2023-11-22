using SCP.Application.Common;
using SCP.Application.Common.Exceptions;
using SCP.Domain.Entity;
using SCP.Domain;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Services;
using SCP.Application.Common.Response;

namespace SCP.Application.Core.UserAuth
{
    public class UserAuthCore : BaseCore
    {
        private readonly UserManager<AppUser> userManager;
        private readonly JwtService jwtService;

        public UserAuthCore(UserManager<AppUser> userManager, JwtService jwtService)
        {
            this.userManager = userManager;
            this.jwtService = jwtService;
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
                    return new CoreResponse<bool>(true);
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

            var pwIsVerifyed = await userManager.CheckPasswordAsync(user, query.Password);
            if (pwIsVerifyed == false)
            {
                return Bad<AuthResponse>("Логин или пароль не верный");
            }

            var token = await jwtService.GenerateJwtToken(user);

            return Good(new AuthResponse { 
                UserId = user.Id.ToString(),
                Jwt = token,
                UserName = user.UserName!
            });
        }
    }
}
