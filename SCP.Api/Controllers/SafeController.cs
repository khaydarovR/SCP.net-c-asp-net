using Microsoft.AspNetCore.Mvc;
using SCP.Api.DTO;
using SCP.Api.Middleware;
using Mapster;
using SCP.Application.Core.Safe;
using SCP.Api.Controllers.Base;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SafeController : CustomController
    {
        private readonly SafeCore safeCore;

        public SafeController(SafeCore safeCore)
        {
            this.safeCore = safeCore;
        }

        /// <summary>
        /// Создание собственного сейфа
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(nameof(CreateMySafe))]
        public async Task<ActionResult> CreateMySafe(CreateSafeDTO dto)
        {
            var command = dto.Adapt<CreateSafeCommand>();
            command.UserId = ContextUserId;
            var res = await safeCore.CreateUserSafe(command);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        /// <summary>
        /// Получение всех сейфов текущего пользователя (или пользователя по id)
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(GetMySafes))]
        public async Task<ActionResult> GetMySafes([FromQuery] string? userId)
        {

            return Ok();
        }
    }
}
