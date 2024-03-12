using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Security.Claims;

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
                    _ = dbContext.AppUsers.Remove(user);
                    Console.WriteLine("================Deleted " + user.Email);
                }
            }
            _ = dbContext.SaveChanges();
            return this;
        }


        public async Task<SystemEntitySeeding> ClearUser(string email)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user != null)
            {
                _ = dbContext.AppUsers.Remove(user);
                Console.WriteLine("================Deleted " + user.Email);
            }

            _ = dbContext.SaveChanges();
            return this;
        }

        /// <summary>
        /// tu1@m.r:q123
        /// </summary>
        /// <param name="ammount"></param>
        /// <returns></returns>
        public async Task<SystemEntitySeeding> InitTUsersWithSafe(int ammount)
        {

            for (int i = 0; i < ammount; i++)
            {
                var u = new AppUser { Email = $"tu{i}@mail.ru", UserName = $"tu{i}_name", TwoFactorEnabled = false };

                if (dbContext.AppUsers.Any(db => db.Email == u.Email))
                {
                    continue;
                }

                users.Add(u);
                var res = await userManager.CreateAsync(u, "q123");

                if (res.Succeeded)
                {
                    _ = await userManager.AddClaimAsync(u, new Claim(ClaimTypes.Role, "User"));
                }

                await GenSafeForUsers(u);
            }
            Console.WriteLine("==============Users inited " + ammount);

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

            Console.WriteLine("=======================Users safe inited " + users.Count);
        }
    }
}
