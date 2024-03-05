
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

namespace SCP.Api.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class CustomController : ControllerBase
    {

        internal Guid ContextUserId { get
            {
                var c = User.Claims.ToList();
                var cv = c.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var res = Guid.Parse(cv);
                return res;
            } }
        internal string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? null;

    }
}
