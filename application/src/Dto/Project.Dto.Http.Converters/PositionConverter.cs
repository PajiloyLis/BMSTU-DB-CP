using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Position;
using Project.Dto.Http.Position;

namespace Project.Dto.Http.Converters;

public static class PositionConverter
{
    [return: NotNullIfNotNull(nameof(position))]
    public static PositionDto? Convert(BasePosition? position)
    {
        if (position is null)
            return null;

        return new PositionDto(position.Id,
            position.ParentId,
            position.Title,
            position.CompanyId
        );
    }
}