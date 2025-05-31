namespace Project.Core.Models.PositionHistory;

public class PositionHistoryPage
{
    public PositionHistoryPage(IReadOnlyList<BasePositionHistory> items, Page page)
    {
        Items = items;
        Page = page;
    }

    public IReadOnlyList<BasePositionHistory> Items { get; set; }
    public Page Page { get; set; }
}