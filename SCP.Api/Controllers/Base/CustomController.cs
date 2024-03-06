
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using SCP.Application.Common.Helpers;
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
                return Helpers.GetId(User);
            } }
        internal string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? null;

    }
}
