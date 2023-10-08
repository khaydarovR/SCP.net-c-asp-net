using Microsoft.AspNetCore.Mvc;
using SCP.Api.DTO;
using SCP.Api.Middleware;
using SCP.Application.Core.Safens.Commands;
using Mapster;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SafeController : CustomController
    {
        public SafeController()
        {

        }

        [HttpPost(nameof(CreateMySafe))]
        public async Task<ActionResult> CreateMySafe(CreateSafeDTO dto)
        {
            var command = dto.Adapt<CreateSafeCommand>();
            await Mediator.Send(command);
            return Ok();
        }

    }
}
