using System.Reflection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace Teams.API.Layer
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddApiDI(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                });

            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc(
                    configuration["ApiVersion"],
                    new OpenApiInfo
                    {
                        Title = "Team Management service | Api",
                        Description = "An ASP.NET Core Web API for managing Teams",
                        Version = configuration["ApiVersion"],
                        Contact = new OpenApiContact
                        {
                            Name = "Artur Lambo",
                            Email = "lamboartur94@gmail.com",
                        },
                    }
                );
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            return services;
        }
    }
}
