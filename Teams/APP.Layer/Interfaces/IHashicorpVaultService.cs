namespace Teams.APP.Layer.Interfaces;
public interface IHashicorpVaultService
{
    Task<string> GetRabbitConnectionStringFromVault();
}