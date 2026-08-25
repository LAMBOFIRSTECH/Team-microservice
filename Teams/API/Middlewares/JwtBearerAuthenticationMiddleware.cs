using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using CustomVaultPackage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Teams.API.Middlewares;

public class JwtBearerAuthenticationMiddleware : AuthenticationHandler<JwtBearerOptions>
{
    // Nom explicite : c'est la clé PUBLIQUE de vérification RSA, pas une clé de signature.
    private const string SigningKeyCacheKey = "Jwt:PublicVerificationKey";

    private readonly HashicorpVaultService _vault;
    private readonly IMemoryCache _cache;
    public JwtBearerAuthenticationMiddleware(HashicorpVaultService vault, IMemoryCache cache, IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory log, UrlEncoder encoder) : base(options, log, encoder)
    {
        _vault = vault;
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            Logger.LogWarning("Authorization header is missing");
            return AuthenticateResult.Fail("Authorization header is missing");
        }

        AuthenticationHeaderValue authHeader;
        try
        {
            authHeader = AuthenticationHeaderValue.Parse(authHeaderValue!);
        }
        catch (FormatException)
        {
            Logger.LogWarning("Malformed Authorization header");
            return AuthenticateResult.Fail("Invalid Authorization header");
        }

        if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning("Invalid authentication scheme: {Scheme}", authHeader.Scheme);
            return AuthenticateResult.Fail("Invalid authentication scheme");
        }

        var jwtToken = authHeader.Parameter;
        if (string.IsNullOrEmpty(jwtToken))
        {
            Logger.LogWarning("Token is missing");
            return AuthenticateResult.Fail("Token is missing");
        }

        RsaSecurityKey verificationKey;
        try
        {
            verificationKey = await GetVerificationKeyAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Login AppRole + lecture secret échoués côté Vault : panne d'infra,
            // distincte d'un token invalide.
            Logger.LogError(ex, "Unable to retrieve JWT public key from Vault");
            return AuthenticateResult.Fail("Authentication temporarily unavailable");
        }

        // Clone impératif : Options.TokenValidationParameters est un singleton partagé
        // entre toutes les requêtes. Le muter directement provoque des races conditions.
        var validationParameters = Options.TokenValidationParameters.Clone();
        validationParameters.IssuerSigningKey = verificationKey;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out _);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (SecurityTokenException ex)
        {
            Logger.LogWarning(ex, "JWT validation failed");
            return AuthenticateResult.Fail("Invalid token");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during JWT validation");
            return AuthenticateResult.Fail("Authentication failed");
        }
    }
    private async Task<RsaSecurityKey> GetVerificationKeyAsync()
    {
        if (_cache.TryGetValue(SigningKeyCacheKey, out RsaSecurityKey? cachedKey) && cachedKey is not null)
        {
            return cachedKey;
        }

        // Coûteux : ça déclenche un login AppRole complet + lecture secret chez Vault.
        // Le cache n'est donc pas juste une optimisation ici, il est indispensable
        // pour ne pas surcharger / faire throttle le serveur Vault sous charge.
        var key = await _vault.GetJwtSigningKeyFromVaultServer().ConfigureAwait(false);

        _cache.Set(SigningKeyCacheKey, key, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20)
        });

        return key;
    }
    
    // /// <summary>
    // /// On récupère la clé publique et le TTL depuis vault sera stocké dans le memory en fonction de ttl
    // /// On pourra déclencher un fallback pour une reauthentification
    // /// </summary>
    // /// <returns></returns>
    // private async Task<RsaSecurityKey> GetVerificationKeyAsync()
    // {
    //     if (_cache.TryGetValue(SigningKeyCacheKey, out RsaSecurityKey? cachedKey) && cachedKey is not null)
    //     {
    //         return cachedKey;
    //     }

    //     var result = await _vault.GetJwtSigningKeyFromVaultServer().ConfigureAwait(false);

    //     _cache.Set(SigningKeyCacheKey, result.Key, new MemoryCacheEntryOptions
    //     {
    //         AbsoluteExpirationRelativeToNow = result.Ttl
    //     });

    //     return result.Key;
    // }
}