namespace Project.Core.Models.PositionHistory;

public class PositionHierarchyWithEmployeePage
{
    public PositionHierarchyWithEmployeePage(IReadOnlyList<PositionHierarchyWithEmployee> items, Page page)
    {
        Items = items;
        Page = page;
    }

    public IReadOnlyList<PositionHierarchyWithEmployee> Items { get; set; }
    public Page Page { get; set; }
}