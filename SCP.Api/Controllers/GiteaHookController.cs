using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Application.Core.GiteaHook;
using SCP.Application.Core.UserInf;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiteaHook : CustomController
    {
        private readonly UserCore userCore;
        private readonly GiteaHookCore giteaHookCore;

        public GiteaHook(UserCore userCore, GiteaHookCore giteaHookCore)
        {
            this.userCore = userCore;
            this.giteaHookCore = giteaHookCore;
        }


        [HttpGet]
        public async Task<ActionResult<bool>> Create([FromBody] dynamic payload)
        {
            var res = await giteaHookCore.CreateHook(ContextUserId.ToString());
            return res.Data;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Catch(string payload)
        {
            //code for get all additional text


            return Ok();
        }
    }
}
