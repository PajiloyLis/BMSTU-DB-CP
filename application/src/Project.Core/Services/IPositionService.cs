using Project.Core.Models;
using Project.Core.Models.Position;
using Project.Core.Models.PositionHistory;

namespace Project.Core.Services;

public interface IPositionService
{
    Task<BasePosition> AddPositionAsync(Guid? parentId, string title, Guid companyId);
    Task<BasePosition> GetPositionByIdAsync(Guid id);
    Task<BasePosition> GetHeadPositionByCompanyIdAsync(Guid id);
    Task<BasePosition> UpdatePositionTitleAsync(Guid id, Guid companyId, Guid? parentId = null, string? title = null);
    Task<BasePosition> UpdatePositionParentWithSubordinatesAsync(Guid id, Guid companyId, Guid? parentId = null, string? title = null);
    Task<BasePosition> UpdatePositionParentWithoutSuboridnatesAsync(Guid id, Guid companyId, Guid? parentId = null, string? title = null);
    Task DeletePositionAsync(Guid id);
    Task<PositionHierarchyPage> GetSubordinatesAsync(Guid parentId, int pageNumber, int pageSize);
}