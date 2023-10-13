
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SCP.Api.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class CustomController : ControllerBase
    {

        internal Guid ContextUserId => !User.Identity.IsAuthenticated
            ? Guid.Empty
            : Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.NameId)!);
    }
}
