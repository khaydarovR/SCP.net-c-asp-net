using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SCP.Application.Common;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.Exceptions;
using SCP.Application.Core.UserAuth.Queries;
using SCP.Application.Services;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.UserAuth.Queries
{
    public class GetJWTQueryHandler : IRequestHandler<GetJWTQuery, string>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IOptions<MyOptions> options;
        private readonly JwtService jwtService;

        public GetJWTQueryHandler(UserManager<AppUser> userManager, IOptions<MyOptions> options, JwtService jwtService)
        {
            this.userManager = userManager;
            this.options = options;
            this.jwtService = jwtService;
        }
        public async Task<string> Handle(GetJWTQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BLException(System.Net.HttpStatusCode.BadRequest ,"Логин или пароль не верный");
            }

            var pwIsVerifyed = await userManager.CheckPasswordAsync(user, request.Password);
            if (pwIsVerifyed == false)
            {
                throw new BLException(System.Net.HttpStatusCode.BadRequest, "Логин или пароль не верный");
            }

            // authentication successful so generate jwt token
            var token = await jwtService.GenerateJwtToken(user);

            return token;
        }

 
    }
}
