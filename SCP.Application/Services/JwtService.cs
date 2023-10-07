using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SCP.Application.Common.Configuration;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Services
{
    public class JwtService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IOptions<MyOptions> options;

        public JwtService(UserManager<AppUser> userManager, IOptions<MyOptions> options)
        {
            this.userManager = userManager;
            this.options = options;
        }


        public async Task<string> GenerateJwtToken(AppUser user)
        {
            // generate token that is valid for 1 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Value.JWT_KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, await GetRoleFromClaims(user)),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GetRoleFromClaims(AppUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var role = claims.First(c => c.Type == ClaimTypes.Role).Value;
            return role;
        }
    }
}
