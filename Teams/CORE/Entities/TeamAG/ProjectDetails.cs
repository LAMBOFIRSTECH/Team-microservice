using Teams.CORE;
namespace Teams.CORE.Entities.TeamAG;
public sealed record ProjectDetails
{
    public enum ProjectDetailState
    {
        /// <summary>
        /// The project is currently active and operational. in project microservice.
        /// </summary>
        Active = 0,
        /// <summary>
        /// The project has been suspended, indicating that it is temporarily inactive or on hold.
        /// This state may require further action or review before the project can be resumed.
        /// </summary>
        Suspended = 1
    }
    public string ProjectName { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public ProjectDetailState State { get; init; }
    public DateTimeOffset? SuspendedAt { get; init; }

    internal ProjectDetails(string projectName, DateTimeOffset startDate, DateTimeOffset endDate, ProjectDetailState state, DateTimeOffset? suspendedAt = null)
    {
        if (string.IsNullOrWhiteSpace(projectName)) throw new BusinessRuleException(Errors.ErrorNature.Validation,"Project name cannot be empty.", "project_name_required");
        if (startDate == DateTimeOffset.MinValue || endDate == DateTimeOffset.MinValue) throw new BusinessRuleException(Errors.ErrorNature.Validation,"Invalid dates.", "invalid_dates");
        if (endDate <= startDate) throw new BusinessRuleException(Errors.ErrorNature.Validation,"End date must be after start date.", "end_date_after_start_date");

        if (state == ProjectDetailState.Suspended && !suspendedAt.HasValue)
            throw new BusinessRuleException(Errors.ErrorNature.Validation,"A suspended project must have a suspension date.", "suspension_date_required");
        if (state == ProjectDetailState.Active && suspendedAt.HasValue)
            throw new BusinessRuleException(Errors.ErrorNature.Validation,"An active project cannot have a suspension date.", "active_project_cannot_have_suspension_date");

        ProjectName = projectName;
        StartDate = startDate;
        EndDate = endDate;
        State = state;
        SuspendedAt = suspendedAt;
    }
}
