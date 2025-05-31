namespace Project.Dto.Http.Score;

public class ScoreDto
{
    public ScoreDto(Guid id, Guid employeeId, Guid authorId, Guid positionId, DateTimeOffset createdAt,
        int efficiencyScore,
        int engagementScore, int competencyScore)
    {
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