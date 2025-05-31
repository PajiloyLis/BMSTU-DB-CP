using System.Diagnostics.CodeAnalysis;
using Project.Core.Models;
using Project.Core.Models.Position;

namespace Database.Models.Converters;

public static class PositionConverter
{
    [return: NotNullIfNotNull("position")]
    public static PositionDb? Convert(CreatePosition? position)
    {
        if (position is null)
            return null;

        return new PositionDb(
            Guid.NewGuid(),
            position.ParentId,
            position.Title,
            position.CompanyId
        );
    }

    [return: NotNullIfNotNull("position")]
    public static PositionDb? Convert(BasePosition? position)
    {
        if (position is null)
            return null;

        return new PositionDb(
            position.Id,
            position.ParentId,
            position.Title,
            position.CompanyId
        );
    }

    [return: NotNullIfNotNull("position")]
    public static BasePosition? Convert(PositionDb? position)
    {
        if (position is null)
            return null;

        return new BasePosition(
            position.Id,
            position.ParentId ?? Guid.Empty,
            position.Title,
            position.CompanyId
        );
    }
}