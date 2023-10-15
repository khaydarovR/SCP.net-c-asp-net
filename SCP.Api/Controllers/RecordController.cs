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


        public Task<ActionResult> Create()
        {
            var res = recordCore.
            return Ok();
        }
    }
}
