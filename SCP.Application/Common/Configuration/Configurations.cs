
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCP.Application.Core.Access;
using SCP.Application.Core.Record;
using SCP.Application.Core.Safe;
using SCP.Application.Core.SafeGuard;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Net.NetworkInformation;
using System.Reflection;

namespace SCP.Application.Common.Configuration
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection ConfigureApplication(this IServiceCollection services, IConfiguration config)
        {
            //конфигурация приложения
            services.InjectDB(config);
            services.Configure<MyOptions>(config.GetSection(nameof(MyOptions)));
            services.AddHttpContextAccessor();

            services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserManager<UserManager<AppUser>>()
            .AddErrorDescriber<IdentityMessageRu>();

            services.AddScoped<JwtService>();
            services.AddSingleton<SymmetricCryptoService>();
            services.AddSingleton<AsymmetricCryptoService>();

            services.AddScoped<UserAuthCore>();
            services.AddScoped<SafeCore>();
            services.AddScoped<RecordCore>();
            services.AddScoped<AccessCore>();
            services.AddScoped<SafeGuardCore>();

            return services;
        }
    }
}
