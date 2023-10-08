using MediatR;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Common.Exceptions;
using SCP.Domain;
using SCP.Domain.Entity;
using System.Net;
using System.Security.Claims;

namespace SCP.Application.Core.UserAuth.Comands
{
    /// <summary>
    /// Создание пользователя с claim ом в БД
    /// </summary>
    public class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Unit>
    {
        private readonly UserManager<AppUser> userManager;

        public CreateAccountHandler(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task<Unit> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var model = new AppUser
            {
                UserName = request.UserName,
                Email = request.Email,
            };

            var result = await userManager.CreateAsync(model, request.Password);
            if (result.Succeeded)
            {
                var dbUser = await userManager.FindByEmailAsync(request.Email);
                var claims = new Claim[] {
                    new Claim(ClaimTypes.Role, SystemRoles.User)
                };

                var claimRes = await userManager.AddClaimsAsync(dbUser, claims);
                if (claimRes.Succeeded)
                {
                    return Unit.Value;
                }

                throw new BLException(HttpStatusCode.BadRequest, claimRes.Errors.First().Description);
            }

            throw new BLException(HttpStatusCode.BadRequest, result.Errors.First().Description);
        }
    }
}
