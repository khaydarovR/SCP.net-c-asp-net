using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain;
using SCP.Domain.Entity;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SCP.Api.ConfigureWebApi
{
    public class SystemEntitySeeding
    {
        List<AppUser> users = new List<AppUser>();

        private readonly AppDbContext dbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly AsymmetricCryptoService asymmetricCrypto;
        private readonly SymmetricCryptoService symmetricCrypto;
        private readonly SafeCore safeCore;

        public SystemEntitySeeding(AppDbContext dbContext, 
            UserManager<AppUser> userManager,
            AsymmetricCryptoService asymmetricCrypto,
            SymmetricCryptoService symmetricCrypto,
            SafeCore safeCore)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.asymmetricCrypto = asymmetricCrypto;
            this.symmetricCrypto = symmetricCrypto;
            this.safeCore = safeCore;

        }

        public async Task<SystemEntitySeeding> ClearUser(params string[] names)
        {
            foreach (var n in names)
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == n);
                if (user != null)
                {
                    dbContext.AppUsers.Remove(user);
                }
            }
            dbContext.SaveChanges();    
            return this;
        }

        public async Task<SystemEntitySeeding> InitTUsersWithSafe(int ammount)
        {

            for (int i = 0; i < ammount; i++)
            {
                var u = new AppUser { Email = $"tu{i}@m.r", UserName = $"tu{i}_name" };

                if (dbContext.AppUsers.Any(db => db.Email == u.Email))
                {
                    continue;
                }

                users.Add(u);
                var res = await userManager.CreateAsync(u, "q123");

                if (res.Succeeded)
                {
                    await userManager.AddClaimAsync(u, new Claim(ClaimTypes.Role, "User"));
                }

                await GenSafeForUsers(u);
            }
            Console.WriteLine("Users inited " + ammount);

            return this;
        }

        public async Task GenSafeForUsers(AppUser user)
        {
            var res = await safeCore.CreateUserSafe(new CreateSafeCommand
            {
                ClearKey = "",
                Description = "Сейф по умолчанию",
                Title = $"Сейф {user.UserName}",
                UserId = user.Id,
            });

            if (res.IsSuccess == false)
            {
                throw new Exception("Seeding safe error " + user.Email);
            }

            Console.WriteLine("Users safe inited " + users.Count);
        }
    }
}
