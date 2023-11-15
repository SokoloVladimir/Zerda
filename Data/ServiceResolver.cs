using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public class Configurator
        {
            public IConfiguration Configuration { get; }

            public Configurator(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            public string ApplicationName
            {
                get => Configuration.GetSection("Application:Name").Value
                    ?? "Application";
            }

            public string DbConnectionString
            {
                get => Configuration.GetConnectionString("zerda")
                    ?? "Server=db;Database=db_zerda;Login=zerdauser;Password=zerdapw";
            }

            public ServerVersion DbServerVersion
            {
                get => ServerVersion.Parse(Configuration.GetConnectionString("zerda-mysql-version")
                    ?? "8.2.0-mysql");
            }
        }
    }
}
