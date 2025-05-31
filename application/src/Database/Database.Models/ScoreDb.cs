using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

/// <summary>
/// Database score story model.
/// </summary>
public class ScoreDb
{
    public ScoreDb(
        Guid id,
        Guid employeeId,
        Guid authorId,
        Guid positionId,
        DateTimeOffset createdAt,
        int efficiencyScore,
        int engagementScore,
        int competencyScore)
    {
        if (efficiencyScore <= 0 || efficiencyScore >= 6)
            throw new ArgumentException("EfficiencyScore must be between 1 and 5", nameof(efficiencyScore));

        if (engagementScore <= 0 || engagementScore >= 6)
            throw new ArgumentException("EngagementScore must be between 1 and 5", nameof(engagementScore));

        if (competencyScore <= 0 || competencyScore >= 6)
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

    /// <summary>
    /// Score id.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Employee id.
    /// </summary>
    [ForeignKey(nameof(EmployeeDb))]
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Author id.
    /// </summary>
    [ForeignKey(nameof(EmployeeDb))]
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Position id.
    /// </summary>
    [ForeignKey(nameof(PositionDb))]
    public Guid PositionId { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Efficiency score (1-5).
    /// </summary>
    [Required]
    public int EfficiencyScore { get; set; }

    /// <summary>
    /// Engagement score (1-5).
    /// </summary>
    [Required]
    public int EngagementScore { get; set; }

    /// <summary>
    /// Competency score (1-5).
    /// </summary>
    [Required]
    public int CompetencyScore { get; set; }
}