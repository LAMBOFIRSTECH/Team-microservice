using JwtAuthLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Teams.APP.Layer.Configurations
{
    public static class AuthorizationConfiguration
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "AdminPolicy",
                    policy =>
                        policy
                            .RequireRole(nameof(Rule.Privilege.Administrateur))
                            .RequireAuthenticatedUser()
                            .AddAuthenticationSchemes("JwtAuthorization")
                );

                options.AddPolicy(
                    "ManagerPolicy",
                    policy =>
                        policy
                            .RequireRole(nameof(Rule.Privilege.Manager))
                            .RequireAuthenticatedUser()
                            .AddAuthenticationSchemes("JwtAuthorization")
                );
            });

            return services;
        }
    }
}
