using Data;
using Microsoft.OpenApi.Models;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ServiceResolver.Configurator configurator = new ServiceResolver.Configurator(builder.Configuration);

            ServiceResolver.AddZerdaDbContext(builder.Services, configurator);
            builder.Services.AddSingleton(configurator);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = configurator.ApplicationName,
                    Description = "RESTful API 'Zerda' component of 'Vulpes' software"
                });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Web.xml"));

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"Type JWT auth.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                      },
                      new List<string>()
                    }
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI(o =>
                {
                    o.DocumentTitle = configurator.ApplicationName;
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}