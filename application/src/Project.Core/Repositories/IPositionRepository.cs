using Project.Core.Models;
using Project.Core.Models.Position;
using Project.Core.Models.PositionHistory;

namespace Project.Core.Repositories;

public interface IPositionRepository
{
    Task<BasePosition> AddPositionAsync(CreatePosition position);
    Task<BasePosition> GetPositionByIdAsync(Guid id);
    Task<BasePosition> UpdatePositionAsync(UpdatePosition position);
    Task DeletePositionAsync(Guid id);
    Task<PositionHierarchyPage> GetSubordinatesAsync(Guid parentId, int pageNumber, int pageSize);
}