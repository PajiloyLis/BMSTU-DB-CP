namespace Project.Core.Models.Score;

public class BaseScore
{
    public BaseScore(Guid id, Guid employeeId, Guid authorId, Guid positionId, DateTimeOffset createdAt,
        int efficiencyScore,
        int engagementScore, int competencyScore)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty", nameof(employeeId));

        if (authorId == Guid.Empty)
            throw new ArgumentException("AuthorId cannot be empty", nameof(authorId));

        if (positionId == Guid.Empty)
            throw new ArgumentException("PositionId cannot be empty", nameof(positionId));

        if (createdAt > DateTimeOffset.UtcNow)
            throw new ArgumentException("CreatedAt cannot be in the future", nameof(createdAt));

        if (efficiencyScore < 1 || efficiencyScore > 5)
            throw new ArgumentException("EfficiencyScore must be between 1 and 5", nameof(efficiencyScore));

        if (engagementScore < 1 || engagementScore > 5)
            throw new ArgumentException("EngagementScore must be between 1 and 5", nameof(engagementScore));

        if (competencyScore < 1 || competencyScore > 5)
            throw new ArgumentException("CompetencyScore must be between 1 and 5", nameof(competencyScore));

        Id = id;
        EmployeeId = employeeId;
        AuthorId = authorId;
        PositionId = positionId;
        CreatedAt = createdAt;
        EfficiencyScore = efficiencyScore;
        EngagementScore = engagementScore;
        CompetencyScore = competencyScore;
    }

    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PositionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int EfficiencyScore { get; set; }
    public int EngagementScore { get; set; }
    public int CompetencyScore { get; set; }
}