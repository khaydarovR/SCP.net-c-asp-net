using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.Record;

namespace SCP.Api.Controllers
{
    public class RecordController : CustomController
    {
        private readonly RecordCore recordCore;

        public RecordController(RecordCore recordCore)
        {
            this.recordCore = recordCore;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string safeId)
        {
            var res = await recordCore.GetAllRecord(safeId, ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        /// <summary>
        /// Чтение секрета на основании прав учетной записи
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Read([FromBody] ReadRecordDTO dto)
        {
            var comand = new ReadRecordCommand
            {
                PubKeyFromClient = dto.PubKey,
                RecordId = Guid.Parse(dto.RecId),
                AuthorId = ContextUserId
            };
            var res = await recordCore.ReadRecord(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        /// <summary>
        /// Импорт записей из Excel
        /// </summary>
        /// <param name="safeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost(nameof(Import))]
        public async Task<IActionResult> Import([FromQuery] Guid safeId, [FromForm] IFormFile file)
        {
            var res = await recordCore.ImportFromExcel(safeId.ToString(), ContextUserId, file);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        /// <summary>
        /// Чтение записи на оснвании Api ключа
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="recId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ReadWithKey([FromHeader(Name = "Api-Key")] string apiKey, [FromQuery] ReadRecordDTO dto)
        {
            var command = new ReadRecordWithKeyCommand();
            command.AuthorId = ContextUserId;
            command.RecordId = dto.RecId;
            command.PubKeyFromClient = dto.PubKey;

            var res = await recordCore.ReadWithKey(apiKey, command);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        /// <summary>
        /// Получение самого подходящей записи по ресурсу или @UserName
        /// </summary>
        /// <param name="forRes"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ReadMatch([FromHeader(Name = "For-Res")] string forRes)
        {
            var res = await recordCore.ReadBestMatch(forRes, ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecordDTO dto)
        {
            var comand = dto.Adapt<CreateRecordCommand>();
            comand.UserId = ContextUserId.ToString();
            var res = await recordCore.CreateRecord(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] PatchRecordDTO dto)
        {
            var comand = dto.Adapt<PatchRecordCommand>();
            comand.UserId = ContextUserId.ToString();
            var res = await recordCore.PatchRecord(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpGet]
        public async Task<IActionResult> Logs(string rId)
        {
            var res = await recordCore.GetLogs(Guid.Parse(rId), ContextUserId);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
    }
}
