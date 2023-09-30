using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SCP.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Common
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            //конфигурация приложения
            services.InjectDB(config);

            return services;
        }
    }
}
