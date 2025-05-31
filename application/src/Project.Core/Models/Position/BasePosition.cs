namespace Project.Core.Models.Position;

public class BasePosition
{
    public BasePosition(Guid id, Guid parentId, string title, Guid companyId)
    {
        if (!Guid.TryParse(id.ToString(), out _)) throw new ArgumentException("Invalid Id format", nameof(id));

        if (!Guid.TryParse(parentId.ToString(), out _))
            throw new ArgumentException("Invalid ParentId format", nameof(parentId));

        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("Invalid CompanyId format", nameof(companyId));

        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty", nameof(title));

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