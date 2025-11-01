namespace Teams.CORE.Layer.Exceptions;
public class ConflictException : DomainException
{
    public ConflictException(string reason, string entity)
        : base(reason, 409, $"Conflict on {entity}")
    { }
}