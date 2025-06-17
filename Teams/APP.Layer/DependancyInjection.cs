using System.Reflection;
using FluentValidation;
using Teams.APP.Layer.CQRS.Validators;
using Teams.APP.Layer.Services;
using Teams.API.Layer.Mappings;

namespace Teams.APP.Layer;

public static class DependancyInjection
{
    public static IServiceCollection AddApplicationDI(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddValidatorsFromAssemblyContaining<CreateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddAutoMapper(typeof(TeamProfile).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        services.AddScoped<EmployeeService>();
        return services;
    }
}
