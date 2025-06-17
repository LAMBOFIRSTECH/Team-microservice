using System;
using Microsoft.Extensions.DependencyInjection;

namespace Teams.CORE.Layer;
public static class DependancyInjection
{
    public static IServiceCollection AddCoreDI(this IServiceCollection services)
    {
        return services;
    }
}
