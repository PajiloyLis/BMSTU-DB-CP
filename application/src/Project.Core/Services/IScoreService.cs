using Project.Core.Models.Score;

namespace Project.Core.Services;

public interface IScoreService
{
    Task<BaseScore> AddScoreAsync(Guid employeeId, Guid authorId, Guid positionId, DateTimeOffset createdAt,
        int efficiencyScore, int engagementScore, int competencyScore);
    Task<BaseScore> UpdateScoreAsync(Guid id, DateTimeOffset? createdAt, int? efficiencyScore, int? engagementScore,
        int? competencyScore);
    Task<BaseScore> GetScoreAsync(Guid id);
    Task<ScorePage> GetScoresAsync(int pageNumber, int pageSize, DateTimeOffset? startDate, DateTimeOffset? endDate);

    Task<ScorePage> GetScoresByEmployeeIdAsync(Guid employeeId, int page, int pageSize, DateTimeOffset? startDate,
        DateTimeOffset? endDate);

    Task<ScorePage> GetScoresByAuthorIdAsync(Guid authorId, int page, int pageSize, DateTimeOffset? startDate,
        DateTimeOffset? endDate);

    Task<ScorePage> GetScoresSubordinatesByEmployeeAsync(Guid employeeId, int page, int pageSize,
        DateTimeOffset? startDate, DateTimeOffset? endDate);

    Task<ScorePage> GetScoresByPositionIdAsync(Guid positionId, int page, int pageSize, DateTimeOffset? startDate,
        DateTimeOffset? endDate);

    Task DeleteScoreAsync(Guid id);
}