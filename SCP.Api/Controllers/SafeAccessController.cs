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
    public class SafeAccessController : CustomController
    {
        private readonly AccessCore accessCore;

        public SafeAccessController(AccessCore accessCore)
        {
            this.accessCore = accessCore;
        }


        [HttpGet("Permisions")]
        public ActionResult Permisions()
        {
            var res = accessCore.GetSystemPermisions();
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpPost(nameof(InviteRequest))]
        public async Task<IActionResult> InviteRequest([FromBody] InviteRequestDTO dto) 
        {
            var comand = dto.Adapt<AuthrizeUsersToSafeCommand>();
            var res = await accessCore.AuthorizeUsersToSafes(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        /// <summary>
        /// Поиск пользователей связанных с текущим пользователем
        /// currentUserId - SafeUsers - Safes - SafeUsers - Users
        /// </summary>
        /// <returns></returns> 
        [HttpGet(nameof(GetLinkedUsers))]
        public async Task<IActionResult> GetLinkedUsers()
        {
            var res = await accessCore.GetLinkedUsersFromSafes(ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
        
    }
}
