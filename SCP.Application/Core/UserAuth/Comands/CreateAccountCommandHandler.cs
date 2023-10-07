using MediatR;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Common;
using SCP.Application.Common.Exceptions;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.UserAuth.Comands
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
    {
        private readonly UserManager<AppUser> userManager;

        public CreateAccountCommandHandler(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
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
                    return dbUser.Id;
                }

                throw new BLException(HttpStatusCode.BadRequest, claimRes.Errors.First().Description);
            }

            throw new BLException(HttpStatusCode.BadRequest, result.Errors.First().Description);
        }
    }
}
