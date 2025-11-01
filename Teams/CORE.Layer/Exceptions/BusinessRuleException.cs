namespace Teams.CORE.Layer.Exceptions;
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string rule, string? detail = null)
        : base(detail ?? $"Business rule violated: {rule}", 422, "Business Rule Violation")
    { }
}