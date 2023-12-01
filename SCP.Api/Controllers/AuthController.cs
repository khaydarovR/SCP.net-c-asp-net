using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    [ApiController]
    public class AuthController : CustomController
    {
        private readonly UserAuthCore userAuthCore;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly EmailService email;

        public AuthController(UserAuthCore userAuthCore, TwoFactorAuthService twoFactorAuthService, EmailService email)
        {
            this.userAuthCore = userAuthCore;
            this.twoFactorAuthService = twoFactorAuthService;
            this.email = email;
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


        [HttpPost(nameof(Activ2FA))]
        public async Task<ActionResult<string>> Activ2FA(string uId, bool isOn)
        {
            var res = await userAuthCore.Activate2FA(uId, isOn);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }


        [HttpPost(nameof(Code2FA))]
        public async Task<ActionResult<string>> Code2FA(string email)
        {
            var res = await userAuthCore.Send2FA(email);
            return res.IsSuccess ? Ok(res.Data) : BadRequest(res.ErrorList);
        }

        [HttpPost(nameof(Test))]
        public async Task<ActionResult<string>> Test()
        {
            await email.SendEmailAsync("razil.khayka@gmail.com", "Test", "Test msg");
            return Ok();
        }

    }
}
