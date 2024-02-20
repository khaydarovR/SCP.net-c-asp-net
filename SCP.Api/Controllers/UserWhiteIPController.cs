using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.UserWhiteIP;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserWhiteIPController : CustomController
    {
        private readonly UserWhiteIPCore whiteIPCore;

        public UserWhiteIPController(UserWhiteIPCore whiteIPCore)
        {
            this.whiteIPCore = whiteIPCore;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromQuery] string ipAddress)
        {
            var res = await whiteIPCore.Create(ContextUserId, ipAddress);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromQuery] string ipId)
        {
            var res = await whiteIPCore.Delete(ContextUserId, ipId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var res = await whiteIPCore.GetAllAllowIP(ContextUserId);
            var r = User.Claims.ToList();
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
    }
}
