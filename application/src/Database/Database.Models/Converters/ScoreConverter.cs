using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Score;

namespace Database.Models.Converters;

public static class ScoreConverter
{
    [return: NotNullIfNotNull(nameof(score))]
    public static ScoreDb? Convert(CreateScore? score)
    {
        if (score == null)
            return null;

        return new ScoreDb(
            Guid.NewGuid(),
            score.EmployeeId,
            score.AuthorId,
            score.PositionId,
            DateTime.UtcNow,
            score.EfficiencyScore,
            score.EngagementScore,
            score.CompetencyScore
        );
    }

    [return: NotNullIfNotNull(nameof(score))]
    public static ScoreDb? Convert(BaseScore? score)
    {
        if (score == null)
            return null;

        return new ScoreDb(
            score.Id,
            score.EmployeeId,
            score.AuthorId,
            score.PositionId,
            score.CreatedAt.ToUniversalTime(),
            score.EfficiencyScore,
            score.EngagementScore,
            score.CompetencyScore
        );
    }

    [return: NotNullIfNotNull(nameof(score))]
    public static BaseScore? Convert(ScoreDb? score)
    {
        if (score == null)
            return null;

        return new BaseScore(
            score.Id,
            score.EmployeeId,
            score.AuthorId,
            score.PositionId,
            score.CreatedAt.ToLocalTime(),
            score.EfficiencyScore,
            score.EngagementScore,
            score.CompetencyScore
        );
    }
}