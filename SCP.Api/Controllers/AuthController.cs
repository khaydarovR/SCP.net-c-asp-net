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
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Guid>> SignUp([FromBody] CreateAccountCommand createAccount)
        {
            var command = createAccount;
            var guid = await Mediator.Send(command);
            return Ok(guid);
        }

        [HttpPost(nameof(SignIn))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<Guid>> SignIn([FromBody] GetJWTQuery getJWTQuery)
        {
            var query = getJWTQuery;
            var token = await Mediator.Send(query);
            return Ok(token);
        }

    }
}
