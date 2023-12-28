using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Data
{
    public static class ServiceResolver
    {
        public static IServiceCollection AddZerdaDbContext(IServiceCollection services, Configurator configurator)
        {
            return services.AddDbContext<ZerdaContext>(o => o.UseMySql(
                    configurator.DbConnectionString,
                    configurator.DbServerVersion)
            );
        }        
    }
}
