namespace Teams.CORE.Errors;

/// <summary>
/// Represents the nature or category of an error that can occur in the system.
/// </summary>
public enum ErrorNature
{
    /// <summary>Invalid input (missing field, incorrect format, validation rule violated).</summary>
    Validation,

    /// <summary>The requested resource/aggregate does not exist.</summary>
    NotFound,

    /// <summary>When the operation conflicts with the current state of the system (duplicate, incompatible state).</summary>
    Conflict,

    /// <summary>When the caller is not authenticated — their identity could not be established.</summary>
    Unauthenticated,

    /// <summary>The caller is authenticated, but does not have the rights for this operation.</summary>
    Forbidden,

    /// <summary>Failure of an external/distant service (timeout, unavailability).</summary>
    RemoteServiceFailure,

    /// <summary>Unexpected error not matching any of the above categories.</summary>
    Unexpected
}