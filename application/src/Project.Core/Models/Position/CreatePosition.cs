namespace Project.Core.Models.Position;

public class CreatePosition
{
    public CreatePosition(Guid? parentId, string title, Guid companyId)
    {
        if (!Guid.TryParse(companyId.ToString(), out _))
            throw new ArgumentException("Invalid CompanyId format", nameof(companyId));

        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty", nameof(Title));

        ParentId = parentId;
        Title = title;
        CompanyId = companyId;
    }

    public Guid? ParentId { get; init; }
    public string Title { get; init; }
    public Guid CompanyId { get; init; }
}