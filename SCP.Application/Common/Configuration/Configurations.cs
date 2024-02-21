using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.OAuth;
using SCP.Application.Core.Record;
using SCP.Application.Core.Safe;
using SCP.Application.Core.SafeGuard;
using SCP.Application.Core.UserAuth;
using SCP.Application.Core.UserInf;
using SCP.Application.Core.UserWhiteIP;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Text;

namespace SCP.Application.Common.Configuration
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection ConfigureApplication(this IServiceCollection services, IConfiguration config)
        {
            //конфигурация приложения
            _ = services.InjectDB(config);
            _ = services.Configure<MyOptions>(config.GetSection(nameof(MyOptions)));
            _ = services.AddHttpClient();

            _ = services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "local";
            });

            _ = services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
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

            _ = services.AddAuthentication(cfg =>
            {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
            {
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.HttpContext.Request.Headers["Authorization"].ToString() ?? null;

                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    }
                };
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
                    ClockSkew = TimeSpan.FromMinutes(5),
                };
            });


            _ = services.AddScoped<JwtService>();
            _ = services.AddSingleton<SymmetricCryptoService>();
            _ = services.AddSingleton<AsymmetricCryptoService>();
            _ = services.AddTransient<EmailService>();
            _ = services.AddTransient<TwoFactorAuthService>();
            _ = services.AddTransient<RLogService>();
            _ = services.Configure<EmailServiceOptions>(config.GetSection("EmailService"));
            _ = services.AddScoped<CacheService>();
            _ = services.AddScoped<UserService>();
            var rabbitMqService = new RabbitMqService(); _ = services.AddSingleton(rabbitMqService);

            _ = services.AddScoped<UserAuthCore>();
            _ = services.AddScoped<SafeCore>();
            _ = services.AddScoped<RecordCore>();
            _ = services.AddScoped<AccessCore>();
            _ = services.AddScoped<SafeGuardCore>();
            _ = services.AddScoped<ApiKeyCore>();
            _ = services.AddScoped<GoogleOAuthCore>();
            _ = services.AddScoped<UserCore>();
            _ = services.AddScoped<GitHubOAuthCore>();
            _ = services.AddScoped<GiteaOAuthCore>();
            _ = services.AddScoped<UserWhiteIPCore>();



            return services;
        }
    }
}
