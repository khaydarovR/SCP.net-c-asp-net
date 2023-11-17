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

        [HttpGet("{recId}")]
        public async Task<IActionResult> GetRecord(string recId, [FromHeader(Name = "Pub-Key")] string clientPublicKey)
        {
            var comand = new ReadRecordCommand { PubKeyFromClient = clientPublicKey, RecordId = Guid.Parse(recId)};
            var res = await recordCore.ReadRecord(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }



        [HttpPost]
        public async Task<IActionResult> CreateRecord([FromBody] CreateRecordDTO dto)
        {
            var comand = dto.Adapt<CreateRecordCommand>();
            comand.UserId = ContextUserId.ToString();
            var res = await recordCore.CreateRecord(comand);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


    }
}
