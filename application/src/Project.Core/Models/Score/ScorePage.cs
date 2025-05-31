namespace Project.Core.Models.Score;

public class ScorePage
{
    public ScorePage()
    {
        Items = new List<BaseScore>();
        Page = new Page();
    }

    public ScorePage(IReadOnlyList<BaseScore> items, Page page)
    {
        Items = items;
        Page = page;
    }

    public IReadOnlyList<BaseScore> Items { get; set; }
    public Page Page { get; set; }
}