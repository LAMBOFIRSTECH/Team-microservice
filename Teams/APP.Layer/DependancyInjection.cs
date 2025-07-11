using System.Reflection;
using FluentValidation;
using Hangfire;
using MediatR;
using Teams.API.Layer.Mappings;
using Teams.APP.Layer.CQRS.Events;
using Teams.APP.Layer.CQRS.Handlers;
using Teams.APP.Layer.CQRS.Validators;
using Teams.APP.Layer.Interfaces;
using Teams.APP.Layer.Services;

namespace Teams.APP.Layer;

public static class DependancyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTeamCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<AddTeamMemberRecordValidator>();
        services.AddValidatorsFromAssemblyContaining<ProjectRecordValidator>();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddAutoMapper(typeof(TeamProfile).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        // services.AddScoped<IEventHandler<EmployeeCreatedEvent>, ManageTeamEventHandler>();
        // services.AddScoped<IEventHandler<ProjectAssociatedEvent>, ManageTeamEventHandler>();
        return services;
    }
}
