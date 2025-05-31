namespace Project.Core.Models.Post;

public class PostPage
{
    public PostPage()
    {
        Posts = new List<BasePost>();
        Page = new Page();
    }

    public PostPage(IReadOnlyList<BasePost> posts, Page page)
    {
        Posts = posts;
        Page = page;
    }

    public IReadOnlyList<BasePost> Posts { get; set; }
    public Page Page { get; set; }
}