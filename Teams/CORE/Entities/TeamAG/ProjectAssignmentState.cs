namespace CORE.Entities.TeamAG;
public enum ProjectAssignmentState
{
    /// <summary>
    /// The project is unassigned to any team. 
    /// </summary>
    Unassigned = 0,
    /// <summary>
    /// The project is assigned to a team and is active in the project microservice.
    /// </summary>
    Assigned = 1,
    /// <summary>
    /// The project is suspended.
    /// </summary>
    Suspended = 2,
    /// <summary>
    /// The project is under review. signifies that the project is being evaluated for its current state and may require action from the team or management.
    /// </summary>
    UnderReview = 3,
    /// <summary>
    /// The project is unassigned after review.
    /// </summary>
    UnassignedAfterReview = 4
}
