using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SCP.DAL
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection InjectDB(this IServiceCollection services, IConfiguration config)
        {
            _ = services.AddDbContext<AppDbContext>(opt =>
            {
                _ = opt.UseNpgsql(config.GetConnectionString("PostgreDb"), b => b.MigrationsAssembly("SCP.DAL"));
            });

            return services;
        }
    }
}
