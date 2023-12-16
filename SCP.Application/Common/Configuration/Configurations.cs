
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.Record;
using SCP.Application.Core.Safe;
using SCP.Application.Core.ApiKey;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Net.NetworkInformation;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace SCP.Application.Common.Configuration
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection ConfigureApplication(this IServiceCollection services, IConfiguration config)
        {
            //конфигурация приложения
            services.InjectDB(config);
            services.Configure<MyOptions>(config.GetSection(nameof(MyOptions)));

            services.AddStackExchangeRedisCache(options => {
                options.Configuration = "localhost";
                options.InstanceName = "local";
            });

            services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.Tokens.EmailConfirmationTokenProvider = "email";
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
                .AddDefaultTokenProviders()
                .AddTokenProvider<Microsoft.AspNetCore.Identity.EmailTokenProvider<AppUser>>("email")
                .AddUserManager<UserManager<AppUser>>()
                .AddErrorDescriber<IdentityMessageRu>();

            //OAuth 2.0 google
            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                options.DefaultScheme = GoogleDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(jwtBearerOptions =>
            {
                var issuer = config.GetValue<string>("MyOptions:JWT_ISSUER");
                var key = config.GetValue<string>("MyOptions:JWT_KEY");
                var keyStr = Encoding.ASCII.GetBytes(key);
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyStr),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            })
            .AddGoogle(googleOptions =>
            {
                
                googleOptions.ClientId = config["GoogleOAuth:ClientId"]!;
                googleOptions.ClientSecret = config["GoogleOAuth:ClientSecret"]!;
                googleOptions.CallbackPath = "/signin-google";
            });


            services.AddScoped<JwtService>();
            services.AddSingleton<SymmetricCryptoService>();
            services.AddSingleton<AsymmetricCryptoService>();
            services.AddTransient<EmailService>();
            services.AddTransient<TwoFactorAuthService>();
            services.AddTransient<RLogService>();
            services.Configure<EmailServiceOptions>(config.GetSection("EmailService"));
            services.AddScoped<CacheService>();

            services.AddScoped<UserAuthCore>();
            services.AddScoped<SafeCore>();
            services.AddScoped<RecordCore>();
            services.AddScoped<AccessCore>();
            services.AddScoped<SafeGuardCore>();
            services.AddScoped<ApiKeyCore>();



            return services;
        }
    }
}
