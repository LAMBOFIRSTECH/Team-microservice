using System.Reflection;
using CustomVaultPackage.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Teams.API.Middlewares;

namespace Teams.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationDI(this IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddMemoryCache();
        services.AddScoped<HashicorpVaultService>();

        services
            .AddAuthentication("JwtAuthorization")
            .AddScheme<JwtBearerOptions, JwtBearerAuthenticationMiddleware>("JwtAuthorization", options =>
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