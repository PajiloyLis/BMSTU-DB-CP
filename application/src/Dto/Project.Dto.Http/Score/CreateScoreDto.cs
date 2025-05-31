namespace Project.Dto.Http.Score;

public class CreateScoreDto
{
    public CreateScoreDto(Guid employeeId, Guid authorId, Guid positionId, DateTimeOffset createdAt,
        int efficiencyScore, int engagementScore, int competencyScore)
    {
        EmployeeId = employeeId;
        AuthorId = authorId;
        PositionId = positionId;
        CreatedAt = createdAt;
        EfficiencyScore = efficiencyScore;
        EngagementScore = engagementScore;
        CompetencyScore = competencyScore;
    }

    public Guid EmployeeId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PositionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int EfficiencyScore { get; set; }
    public int EngagementScore { get; set; }
    public int CompetencyScore { get; set; }
}