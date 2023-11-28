using Microsoft.AspNetCore.Mvc;
using SCP.Api.Middleware;
using SCP.Application.Core.Safe;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using Mapster;
using SCP.Application.Core.UserAuth;
using SCP.Application.Core.SafeGuard;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApiKeyController : CustomController
    {
        private readonly ApiKeyCore keyCore;

        public ApiKeyController(ApiKeyCore keyCore)
        {
            this.keyCore = keyCore;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateApiKeyDTO dto)
        {
            var cmd = dto.Adapt<CreateKeyCommand>();
            cmd.UserId = ContextUserId;
            var res = await keyCore.CreateApiKey(cmd);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpGet("GetMy")]
        public async Task<IActionResult> GetMy()
        {
            var res = await keyCore.GetKeys(ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(string keyId)
        {
            var res = await keyCore.Delete(keyId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
    }
}
