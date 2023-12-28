using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data
{
    public sealed class Configurator
    {
        private IConfiguration _configuration { get; }

        public Configurator(IConfiguration configuration)
        {
            _configuration = configuration;
            JwtOptions = new AuthOptions(configuration);
        }

        public string ApplicationName
        {
            get => _configuration.GetSection("Application:Name").Value
                ?? "Application";
        }

        public string DbConnectionString
        {
            get => _configuration.GetConnectionString("zerda")
                ?? "Server=db;Database=db_zerda;Login=zerdauser;Password=zerdapw";
        }

        public ServerVersion DbServerVersion
        {
            get => ServerVersion.Parse(_configuration.GetConnectionString("zerda-mysql-version")
                ?? "8.2.0-mysql");
        }

        public AuthOptions JwtOptions { get; }

        public class AuthOptions
        {
            public IConfiguration Configuration { get; }

            // public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            //   new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

            public string Issuer
            {
                get => Configuration.GetSection("JsonWebToken:Issuer").Value
                    ?? "Zerda_issuer";
            }
            public int Lifetime
            {
                get
                {
                    int lifetime;
                    if (Int32.TryParse(Configuration.GetSection("JsonWebToken:Lifetime").Value, out lifetime))
                    {
                        return lifetime;
                    }
                    else
                    {
                        return 30;
                    }

                }
            }
            public string Audience
            {
                get => Configuration.GetSection("JsonWebToken:Issuer").Value
                    ?? "Zerda_audience";
            }
            public string Key
            {
                get
                {
                    try
                    {
                        return Configuration.GetRequiredSection("JsonWebToken:Key").Value!;
                    }
                    catch (Exception)
                    {
                        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                        char[] generatedKey = new char[30];

                        for (int i = 0; i < generatedKey.Length; i++)
                        {
                            generatedKey[i] = alphabet[Random.Shared.Next(alphabet.Length)];
                        }

                        Configuration["JsonWebToken:Key"] = new String(generatedKey);
                        return new String(generatedKey);
                    }
                }
            }

            public AuthOptions(IConfiguration configuration)
            {
                Configuration = configuration;
                string generateOnStartup = Key;
            }
        }
    }
}
