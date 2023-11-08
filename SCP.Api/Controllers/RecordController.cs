using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
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
        public async Task<IActionResult> GetSecret(string recId, [FromHeader(Name = "Pub-Key")] string clientPublicKey)
        {
            var comand = new ReadRecordCommand { PubKeyFromClient = clientPublicKey, RecordId = Guid.Parse(recId)};
            var response = await recordCore.ReadRecord(comand);
            return Ok(response);
        }

    }
}
