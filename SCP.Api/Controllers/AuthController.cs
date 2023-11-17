using Mapster;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.UserAuth;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : CustomController
    {
        private readonly UserAuthCore userAuthCore;

        public AuthController(UserAuthCore userAuthCore)
        {
            this.userAuthCore = userAuthCore;
        }


        [HttpPost(nameof(SignUp))]
        public async Task<ActionResult> SignUp([FromBody] CreateAccountDTO dto)
        {
            var command = dto.Adapt<CreateAccountCommand>();
            var res = await userAuthCore.CreateAccount(command);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpGet(nameof(SignIn))]
        public async Task<ActionResult<string>> SignIn([FromQuery] SignInDTO dto)
        {
            var query = dto.Adapt<GetJwtQuery>();
            var res = await userAuthCore.GetJwtAndClaims(query);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

    }
}
