using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teams.API.Layer.Middlewares;
using Teams.API.Layer.Common;

namespace Teams.API.Layer
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddApiDI(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddControllers(options => options.Filters.Add<ValidateModelAttribute>())
                    .ConfigureApiBehaviorOptions(options =>
                        {
                            options.SuppressModelStateInvalidFilter = true;
                        })
                    .AddNewtonsoftJson(options =>
                        {
                            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                            options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
                        })
                    .AddJsonOptions(opt =>
                        {
                            // Ignore la casse lors de la désérialisation
                            opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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
            services
                .AddAuthentication("JwtAuthorization")
                .AddScheme<JwtBearerOptions, JwtBearerAuthenticationMiddleware>(
                    "JwtAuthorization",
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configuration["Jwt:Issuer"],
                            ValidAudience = configuration["Jwt:Audience"],
                        };
                    }
                );
            return services;
        }
    }
}
