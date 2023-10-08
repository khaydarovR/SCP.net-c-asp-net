using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Middleware;
using SCP.Application.Core.UserAuth.Comands;
using SCP.Application.Core.UserAuth.Queries;
using System.Security.Claims;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : CustomController
    {
        public AuthController()
        {

        }


        [HttpPost(nameof(SignUp))]
        public async Task<ActionResult<Unit>> SignUp([FromBody] CreateAccountCommand createAccount)
        {
            var command = createAccount;
            await Mediator.Send(command);
            return Ok();
        }

        [HttpGet(nameof(SignIn))]
        public async Task<ActionResult<string>> SignIn([FromQuery] GetJWTQuery getJWTQuery)
        {
            var query = getJWTQuery;
            var token = await Mediator.Send(query);
            return Ok(token);
        }

    }
}
