namespace Project.Dto.Http.Position;

public class PositionDto
{
    public PositionDto(Guid id, Guid parentId, string title, Guid companyId)
    {
        Id = id;
        ParentId = parentId;
        Title = title;
        CompanyId = companyId;
    }

    public Guid Id { get; init; }
    public Guid ParentId { get; init; }
    public string Title { get; init; }
    public Guid CompanyId { get; init; }
}