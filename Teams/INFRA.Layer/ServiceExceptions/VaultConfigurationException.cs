namespace Teams.INFRA.Layer.ServiceExceptions;
public class VaultConfigurationException : Exception
{
    public VaultConfigurationException(int Status, string Type, string message) : base(message) { }
}