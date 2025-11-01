namespace Teams.CORE.Layer.Exceptions;
public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object id)
        : base($"No {entity.ToLower()} found with identifier '{id}'.", 404, "Resource Not Found")
    { }
}