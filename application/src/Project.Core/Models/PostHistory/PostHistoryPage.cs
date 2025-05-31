namespace Project.Core.Models.PostHistory;

public class PostHistoryPage
{
    public PostHistoryPage(IReadOnlyList<BasePostHistory> items, Page page)
    {
        Items = items;
        Page = page;
    }

    public IReadOnlyList<BasePostHistory> Items { get; set; }
    public Page Page { get; set; }
}