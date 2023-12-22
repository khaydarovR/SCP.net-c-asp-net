using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.UserAuth;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomController
    {
        private readonly UserCore userCore;

        public UserController(UserCore userCore)
        {
            this.userCore = userCore;
        }


        [HttpGet("Info")]
        public async Task<ActionResult<GoogleUserInfoResponse>> Info([FromQuery] string uId)
        {
            var res = await userCore.GetUserInfo(uId);

            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }
    }
}
