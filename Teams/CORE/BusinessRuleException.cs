using Teams.CORE.Errors;

namespace Teams.CORE;
public class BusinessRuleException : Exception, IHasErrorNature
{
    public ErrorNature Nature { get; }
    public string Reason { get; }

    public BusinessRuleException(ErrorNature nature, string reason, string message)
        : base(message)
    {
        Nature = nature;
        Reason = reason;
    }
}