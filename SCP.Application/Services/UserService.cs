using Microsoft.AspNetCore.Identity;
using SCP.Domain.Entity;
using System.Security.Claims;

namespace SCP.Application.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }


        public async Task<string> GetRoleFromClaims(AppUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var role = claims.First(c => c.Type == ClaimTypes.Role).Value;
            return role;
        }

    }
}
