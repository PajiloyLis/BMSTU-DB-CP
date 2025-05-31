namespace Project.Dto.Http.Position;

public class CreatePositionDto
{
    public CreatePositionDto(Guid? parentId, string title, Guid companyId)
    {
        ParentId = parentId;
        Title = title;
        CompanyId = companyId;
    }

    public Guid? ParentId { get; init; }
    public string Title { get; init; }
    public Guid CompanyId { get; init; }
}