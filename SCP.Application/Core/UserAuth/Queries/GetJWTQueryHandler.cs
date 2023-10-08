using MediatR;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Common.Exceptions;
using SCP.Application.Core.UserAuth.Queries;
using SCP.Application.Services;
using SCP.Domain.Entity;

namespace SCP.Application.UserAuth.Queries
{
    /// <summary>
    /// Генерация и отпарвка JWT токена для пользователя
    /// </summary>
    public class GetJWTQueryHandler : IRequestHandler<GetJWTQuery, string>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly JwtService jwtService;

        public GetJWTQueryHandler(UserManager<AppUser> userManager, JwtService jwtService)
        {
            this.userManager = userManager;
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
