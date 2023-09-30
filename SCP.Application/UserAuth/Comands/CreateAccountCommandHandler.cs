using MediatR;
using Microsoft.AspNetCore.Identity;
using SCP.Application.Common;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.UserAuth.Comands
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
                var claimRes = await userManager.AddClaimAsync(dbUser, new Claim(ClaimTypes.Role, SystemRoles.User));
                if (claimRes.Succeeded)
                {
                    return dbUser.Id;
                }

                throw new BLException(claimRes.Errors.First().Description);
            }

            throw new BLException(result.Errors.First().Description);
        }
    }
}
