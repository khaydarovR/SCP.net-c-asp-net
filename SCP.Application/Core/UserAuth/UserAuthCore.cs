using SCP.Application.Common;
using SCP.Application.Common.Exceptions;
using SCP.Domain.Entity;
using SCP.Domain;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Services;

namespace SCP.Application.Core.UserAuth
{
    public class UserAuthCore
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

                new CoreResponse<bool>(claimRes.Errors.Select(e => e.Description));
            }
            return new CoreResponse<bool>(result.Errors.Select(e => e.Description));
        }



        public async Task<CoreResponse<string>> GetJwt(GetJwtQuery query)
        {
            var user = await userManager.FindByEmailAsync(query.Email);
            if (user == null)
            {
                return new CoreResponse<string>("Логин или пароль не верный");
            }

            var pwIsVerifyed = await userManager.CheckPasswordAsync(user, query.Password);
            if (pwIsVerifyed == false)
            {
                return new CoreResponse<string>("Логин или пароль не верный");
            }

            var token = await jwtService.GenerateJwtToken(user);

            return new CoreResponse<string>(token, true);
        }
    }
}
