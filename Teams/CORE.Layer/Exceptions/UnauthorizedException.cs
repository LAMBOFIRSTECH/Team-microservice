namespace Teams.CORE.Layer.Exceptions;
public class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string reason)
        : base(reason, 401, "Unauthorized Access")
    { }
}