using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SCP.Application.Common.Configuration;
using SCP.Domain.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SCP.Application.Services
{
    public class JwtService
    {
        private readonly IOptions<MyOptions> options;
        private readonly UserService userService;

        public JwtService(IOptions<MyOptions> options, UserService userService)
        {
            this.options = options;
            this.userService = userService;
        }


        public async Task<string> GenerateJwtToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(options.Value.JWT_KEY);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, await userService.GetRoleFromClaims(user)),
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = options.Value.JWT_ISSUER,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
