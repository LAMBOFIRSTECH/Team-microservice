using System.Net.Sockets;
using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.Token;
using Teams.INFRA.Layer.ServiceExceptions;
using Teams.APP.Layer.Interfaces;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
namespace Teams.INFRA.Layer.Services;
public class HashicorpVaultService : IHashicorpVaultService
{
    private readonly IConfiguration configuration;
    private readonly ILogger<HashicorpVaultService> log;
    public HashicorpVaultService(IConfiguration configuration, ILogger<HashicorpVaultService> log)
    {
        this.configuration = configuration;
        this.log = log;
    }
    public async Task<string> GetAppRoleTokenFromVault()
    {
        var hashiCorpRoleID = configuration["HashiCorp:AppRole:RoleID"];
        var hashiCorpSecretID = configuration["HashiCorp:AppRole:SecretID"];
        var hashiCorpHttpClient = configuration["HashiCorp:HttpClient:BaseAddress"];
        if (string.IsNullOrEmpty(hashiCorpRoleID) || string.IsNullOrEmpty(hashiCorpSecretID) || string.IsNullOrEmpty(hashiCorpHttpClient))
        {
            log.LogWarning("💢 Empty or invalid HashiCorp Vault configurations.");
            throw new VaultConfigurationException(500, "Warning", "💢 Empty or invalid HashiCorp Vault configurations.");
        }
        var appRoleAuthMethodInfo = new AppRoleAuthMethodInfo(hashiCorpRoleID, hashiCorpSecretID);
        var vaultClientSettings = new VaultClientSettings($"{hashiCorpHttpClient}", appRoleAuthMethodInfo);
        var vaultClient = new VaultClient(vaultClientSettings);
        try
        {
            var authResponse = await vaultClient.V1.Auth.AppRole.LoginAsync(appRoleAuthMethodInfo);
            string token = authResponse.AuthInfo.ClientToken;
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("💢 Empty token retrieve from HashiCorp Vault");
            return token;
        }
        catch (Exception ex) when (ex.InnerException is SocketException socket)
        {
            log.LogError(socket, "💢 Socket's problems check if Hashicorp Vault server is UP");
            throw new InvalidOperationException("💢 The service is unavailable. Please retry soon.", ex);
        }
    }
    public async Task<string> GetRabbitConnectionStringFromVault()
    {
        string vautlAppRoleToken = await GetAppRoleTokenFromVault();
        var secretPath = configuration["HashiCorp:RabbitMqPath"];
        var hashiCorpHttpClient = configuration["HashiCorp:HttpClient:BaseAddress"];
        if (string.IsNullOrEmpty(hashiCorpHttpClient) || string.IsNullOrEmpty(secretPath))
        {
            log.LogWarning("💢 Empty or invalid HashiCorp Vault configurations.");
            throw new VaultConfigurationException(500, "Warning", "💢 Empty or invalid HashiCorp Vault configurations.");
        }
        var vaultClientSettings = new VaultClientSettings($"{hashiCorpHttpClient}", new TokenAuthMethodInfo(vautlAppRoleToken));
        var vaultClient = new VaultClient(vaultClientSettings);
        var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);
        if (secret == null)
        {
            log.LogError("Le secret Vault est introuvable pour la chaine de connection rabbitMQ.");
            throw new VaultConfigurationException(404, "Error", "Le secret Vault est introuvable.");
        }
        var secretData = secret.Data.Data;
        if (!secretData.ContainsKey("rabbitMqConnectionString"))
        {
            log.LogError("❌ La clé 'rabbitMqConnectionString' est manquante dans le secret Vault.");
            throw new VaultConfigurationException(404, "Error", "❌ Key 'rabbitMqConnectionString' not found.");
        }
        return secretData["rabbitMqConnectionString"].ToString()!;
    }
    private async Task<RsaSecurityKey> GetJwtSigningKeyFromVaultServer()
    {
        string vautlAppRoleToken = await GetAppRoleTokenFromVault();
        var hashiCorpHttpClient = configuration["HashiCorp:HttpClient:BaseAddress"];
        if (string.IsNullOrEmpty(vautlAppRoleToken) || string.IsNullOrEmpty(hashiCorpHttpClient))
        {
            log.LogWarning("La configuration de HashiCorp Vault est manquante ou invalide.");
            throw new InvalidOperationException("La configuration de HashiCorp Vault est manquante ou invalide.");
        }
        var vaultClientSettings = new VaultClientSettings($"{hashiCorpHttpClient}", new TokenAuthMethodInfo(vautlAppRoleToken));
        var vaultClient = new VaultClient(vaultClientSettings);
        try
        {
            var secretPath = configuration["HashiCorp:JwtPublicKeyPath"];
            var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath);
            if (secret == null)
            {
                log.LogError("Le secret Vault est introuvable.");
                throw new InvalidOperationException("Le secret Vault est introuvable.");
            }
            var secretData = secret.Data.Data;
            if (!secretData.ContainsKey("authenticationSignatureKey"))
            {
                log.LogError("La clé publique 'authenticationSignatureKey' est manquante dans le secret Vault.");
                throw new InvalidOperationException("La clé publique 'authenticationSignatureKey' est introuvable.");
            }
            string rawPublicKeyPem = secretData["authenticationSignatureKey"].ToString()!;
            rawPublicKeyPem = rawPublicKeyPem.Trim();
            if (!rawPublicKeyPem.Contains("-----BEGIN RSA PUBLIC KEY-----") ||
                !rawPublicKeyPem.Contains("-----END RSA PUBLIC KEY-----"))
            {
                log.LogWarning("La clé récupérée n'a pas le bon format PEM.");
                throw new Exception("La clé récupérée n'a pas le bon format PEM.");
            }
            string keyBody = rawPublicKeyPem
                .Replace("-----BEGIN RSA PUBLIC KEY-----", "")
                .Replace("-----END RSA PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
            if (string.IsNullOrEmpty(keyBody))
            {
                throw new Exception("Le contenu de la clé est vide après le nettoyage.");
            }
            string formattedPublicKeyPem = "-----BEGIN RSA PUBLIC KEY-----\n" +
                string.Join("\n", Enumerable.Range(0, (keyBody.Length + 63) / 64) 
                    .Select(i => keyBody.Substring(i * 64, Math.Min(64, keyBody.Length - (i * 64))))) +
                "\n-----END RSA PUBLIC KEY-----"; // On va devoir regarder le nombre de caractères générés depuis keycloak pour formater la clé correctement
            var rsa = RSA.Create();
            rsa.ImportFromPem(formattedPublicKeyPem);
            var rsaSecurityKey = new RsaSecurityKey(rsa);
            log.LogInformation("La clé publique a été récupérée et formatée avec succès.");
            return rsaSecurityKey;
        }
        catch (FormatException ex)
        {
            log.LogError(ex, "Erreur lors de la conversion de la clé publique Base64 ");
            throw new Exception("Erreur lors de la conversion de la clé publique Base64.", ex);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Erreur lors de la récupération de la clé publique dans Vault");
            throw new Exception("Erreur lors de la récupération de la clé publique dans Vault", ex);
        }
    }
}