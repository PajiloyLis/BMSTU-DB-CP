namespace Project.Core.Models.Position;

public class PositionPage
{
    public PositionPage()
    {
        Positions = new List<BasePosition>();
        Page = new Page();
    }

    public PositionPage(IReadOnlyList<BasePosition> positions, Page page)
    {
        Positions = positions;
        Page = page;
    }

    public IReadOnlyList<BasePosition> Positions { get; set; }
    public Page Page { get; set; }
}