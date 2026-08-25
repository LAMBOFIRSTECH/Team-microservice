using Teams.CORE.Errors;
namespace Teams.APP;

public class AppHandlerException : Exception, IHasErrorNature
{
    public ErrorNature Nature { get; }
    public string Reason { get; }

    public AppHandlerException(ErrorNature nature, string? reason, string message)
        : base(message)
    {
        Nature = nature;
        Reason = reason ?? "app_error";
    }
}