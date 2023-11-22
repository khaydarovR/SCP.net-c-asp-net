using Microsoft.AspNetCore.Mvc;
using SCP.Api.DTO;
using SCP.Api.Middleware;
using Mapster;
using SCP.Application.Core.Safe;
using SCP.Api.Controllers.Base;
using SCP.Application.Core.UserAuth;
using SCP.Application.Core.Record;
using SCP.Application.Core.Access;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccessController : CustomController
    {
        private readonly AccessCore accessCore;

        public AccessController(AccessCore accessCore)
        {
            this.accessCore = accessCore;
        }


        [HttpPost]
        public async Task<IActionResult> InviteUserToSafeAsync([FromBody] InviteRequestDTO dto) 
        {
            var comand = dto.Adapt<AuthrizeUsersToSafeCommand>();
            var res = await accessCore.AuthorizeUsersToSafes(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        
    }
}
