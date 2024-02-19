using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.Access;

namespace SCP.Api.Controllers
{
    /// <summary>
    /// Работа с разрешениями для сейфов
    /// </summary>
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


        /// <summary>
        /// Получение списка системных разрешений со slug ом
        /// </summary>
        /// <returns></returns>
        [HttpGet("Permisions")]
        public ActionResult Permisions()
        {
            var res = accessCore.GetSystemPermisions();
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        /// <summary>
        /// Приглашение пользователя в сейф посредством выдачи ему разрешений
        /// </summary>
        /// <param name="dto">Данные для регистрации пользвателя в сейфе</param>
        /// <returns></returns>
        [HttpPost(nameof(InviteRequest))]
        public async Task<IActionResult> InviteRequest([FromBody] InviteRequestDTO dto)
        {
            var comand = dto.Adapt<AuthrizeUsersToSafeCommand>();
            comand.AuthorId = ContextUserId;
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


        /// <summary>
        /// Получить список разрешений для пользователя в сейфе
        /// </summary>
        /// <param name="uId"></param>
        /// <param name="sId"></param>
        /// <returns></returns>
        [HttpGet(nameof(GetPer))]
        public async Task<IActionResult> GetPer([FromQuery] string uId, [FromQuery] string sId)
        {
            var cmd = new GetPerQuery
            {
                AuthorId = ContextUserId,
                SafeId = Guid.Parse(sId),
                UserId = Guid.Parse(uId)
            };
            var res = accessCore.GetPermissions(cmd);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        /// <summary>
        /// Обновить разрешения в сейфе для пользователя
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(nameof(PatchPer))]
        public async Task<IActionResult> PatchPer([FromBody] PatchPerDTO dto)
        {
            var cmd = dto.Adapt<PatchPerCommand>();
            cmd.AuthorId = ContextUserId;
            var res = await accessCore.UpdatePermissions(cmd);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        /// <summary>
        /// Прикрепление к сейфу с помощью разрешения
        /// поддерживает отложенное приглашение
        /// </summary>
        /// <param name="email"></param>
        /// <param name="safeId"></param>
        /// <returns></returns>
        [HttpPost(nameof(JustInvite))]
        public async Task<IActionResult> JustInvite(string safeId, string email)
        {
            var res = await accessCore.JustInvite(safeId, email, ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
    }
}
