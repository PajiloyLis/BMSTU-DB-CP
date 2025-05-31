using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Position;
using Project.Dto.Http.Position;

namespace Project.Dto.Http.Converters;

public static class PositionHierarchyConverter
{
    [return: NotNullIfNotNull(nameof(positionHierarchy))]
    public static PositionHierarchyDto? Convert(PositionHierarchy? positionHierarchy)
    {
        if (positionHierarchy is null)
            return null;

        return new PositionHierarchyDto(positionHierarchy.PositionId,
            positionHierarchy.ParentId,
            positionHierarchy.Title,
            positionHierarchy.Level
        );
    }
}