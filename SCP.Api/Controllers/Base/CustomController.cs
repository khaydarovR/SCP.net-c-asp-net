
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SCP.Api.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class CustomController : ControllerBase
    {

        internal Guid ContextUserId => !User.Identity.IsAuthenticated
            ? Guid.Empty
            : Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        internal string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? null;

    }
}
