using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SCP.Application.Common.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SCP.Application.Common.PipeLine
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IOptions<MyOptions> options)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault();

            if (token != null)
                AttachUserToContext(context, token, options);

            await _next(context);
        }

        private async void AttachUserToContext(HttpContext context, string token, IOptions<MyOptions> opt)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(opt.Value.JWT_KEY);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = opt.Value.JWT_ISSUER,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var identity = new ClaimsIdentity(jwtToken.Claims, JwtBearerDefaults.AuthenticationScheme);
                context.User = new ClaimsPrincipal(identity);
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
