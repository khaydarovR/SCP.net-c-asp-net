using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SCP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class CustomController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        internal Guid UserId => !User.Identity.IsAuthenticated
            ? Guid.Empty
            : Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
}
