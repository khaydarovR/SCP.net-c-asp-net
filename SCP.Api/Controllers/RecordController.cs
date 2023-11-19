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

        [HttpPost]
        public async Task<IActionResult> Read([FromBody] ReadRecordDTO dto)
        {
            var comand = new ReadRecordCommand { PubKeyFromClient = dto.PubKey, RecordId = Guid.Parse(dto.RecId)};
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


    }
}
