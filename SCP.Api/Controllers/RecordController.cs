using Mapster;
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


        [HttpPost]
        public async Task<IActionResult> Read([FromBody] ReadRecordDTO dto)
        {
            var comand = new ReadRecordCommand { 
                PubKeyFromClient = dto.PubKey, 
                RecordId = Guid.Parse(dto.RecId),
                AuthorId = ContextUserId
            };
            var res = await recordCore.ReadRecord(comand);
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
    }
}
 