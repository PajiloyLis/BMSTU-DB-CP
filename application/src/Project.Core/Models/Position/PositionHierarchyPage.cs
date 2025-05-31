namespace Project.Core.Models.Position;

public class PositionHierarchyPage
{
    public PositionHierarchyPage(IReadOnlyList<PositionHierarchy> items, Page page)
    {
        Items = items;
        Page = page;
    }

    public IReadOnlyList<PositionHierarchy> Items { get; set; }
    public Page Page { get; set; }
}