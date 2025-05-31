using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Score;
using Project.Dto.Http.Score;

namespace Project.Dto.Http.Converters;

public static class ScoreConverter
{
    [return: NotNullIfNotNull(nameof(score))]
    public static ScoreDto? Convert(BaseScore? score)
    {
        if (score is null)
            return null;

        return new ScoreDto(score.Id,
            score.EmployeeId,
            score.AuthorId,
            score.PositionId,
            score.CreatedAt,
            score.EfficiencyScore,
            score.EngagementScore,
            score.CompetencyScore
        );
    }
}