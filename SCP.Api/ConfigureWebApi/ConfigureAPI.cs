using Microsoft.OpenApi.Models;
using System.Reflection;

namespace SCP.Api.ConfigureWebApi
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection ConfigureWebApi(this IServiceCollection services, IConfiguration config)
        {
            //конфигурация swagger auth
            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JSON Web Token based security",
            };
            var securityReq = new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            };
            services.AddSwaggerGen(o =>
            {
                o.AddSecurityDefinition("Bearer", securityScheme);
                o.AddSecurityRequirement(securityReq);
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });

            services.AddTransient<SystemEntitySeeding>();

            return services;
        }
    }
}
