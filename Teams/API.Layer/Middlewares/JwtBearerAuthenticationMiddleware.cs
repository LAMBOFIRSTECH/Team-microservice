using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using CustomVaultPackage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Teams.APP.Layer.Helpers;

namespace Teams.API.Layer.Middlewares;

public class JwtBearerAuthenticationMiddleware : AuthenticationHandler<JwtBearerOptions>
{
    private readonly IConfiguration configuration;

    public JwtBearerAuthenticationMiddleware(
        IConfiguration configuration,
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory log,
        UrlEncoder encoder
    )
        : base(options, log, encoder)
    {
        this.configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            LogHelper.Warning("Authorization header is missing", Logger);
            return await Task.FromResult(
                AuthenticateResult.Fail("Authorization header is missing")
            );
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
            if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                LogHelper.Warning("Invalid authentication scheme", Logger);
                return await Task.FromResult(
                    AuthenticateResult.Fail("Invalid authentication scheme")
                );
            }

            var jwtToken = authHeader.Parameter;
            if (string.IsNullOrEmpty(jwtToken))
            {
                LogHelper.Warning("Token is missing", Logger);
                return AuthenticateResult.Fail("Token is missing");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = Options.TokenValidationParameters;
            var vault = new HashicorpVaultService(configuration);
            validationParameters.IssuerSigningKey = await vault.GetJwtSigningKeyFromVaultServer();
            var principal = tokenHandler.ValidateToken(
                jwtToken,
                validationParameters,
                out SecurityToken securityToken
            );
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            LogHelper.Error($"Authentication failed: {ex.Message}", Logger);
            return await Task.FromResult(
                AuthenticateResult.Fail($"Authentication failed: {ex.Message}")
            );
        }
    }
}
