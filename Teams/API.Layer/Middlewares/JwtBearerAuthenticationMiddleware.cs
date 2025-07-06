using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using CustomVaultPackage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Teams.API.Layer.Middlewares;

public class JwtBearerAuthenticationMiddleware : AuthenticationHandler<JwtBearerOptions>
{
    private readonly IConfiguration configuration;

    public JwtBearerAuthenticationMiddleware(
        IConfiguration configuration,
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder)
    {
        this.configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return await Task.FromResult(AuthenticateResult.Fail("Authorization header missing"));
        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
            if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return await Task.FromResult(
                    AuthenticateResult.Fail("Invalid authentication scheme")
                );

            var jwtToken = authHeader.Parameter;
            if (string.IsNullOrEmpty(jwtToken))
                return AuthenticateResult.Fail("Token is missing.");

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
            return await Task.FromResult(
                AuthenticateResult.Fail($"Authentication failed: {ex.Message}")
            );
        }
    }
}
