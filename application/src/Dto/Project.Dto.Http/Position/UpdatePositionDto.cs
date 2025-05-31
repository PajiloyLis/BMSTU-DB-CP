namespace Project.Dto.Http.Position;

public class UpdatePositionDto
{
    public UpdatePositionDto(Guid id, Guid companyId, Guid? parentId = null, string? title = null)
    {
        if (!Guid.TryParse(id.ToString(), out _)) throw new ArgumentException("Invalid Id format", nameof(id));

        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("Invalid CompanyId format", nameof(companyId));

        if (parentId.HasValue && !Guid.TryParse(parentId.Value.ToString(), out _))
            throw new ArgumentException("Invalid ParentId format", nameof(parentId));

        if (title != null && string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Id = id;
        CompanyId = companyId;
        ParentId = parentId;
        Title = title;
    }

    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public Guid? ParentId { get; init; }
    public string? Title { get; init; }
}