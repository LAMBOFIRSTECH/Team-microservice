using Teams.CORE.Errors;
namespace Teams.INFRA;

public class InfrastructureException : Exception, IHasErrorNature
{
    public ErrorNature Nature { get; }
    public string Reason { get; }

    public InfrastructureException(ErrorNature nature, string? reason, string message)
        : base(message)
    {
        Nature = nature;
        Reason = reason ?? "infra_error";
    }
}