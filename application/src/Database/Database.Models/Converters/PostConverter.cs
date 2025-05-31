using System.Diagnostics.CodeAnalysis;
using Project.Core.Models;
using Project.Core.Models.Post;

namespace Database.Models.Converters;

public static class PostConverter
{
    [return: NotNullIfNotNull(nameof(post))]
    public static PostDb? Convert(CreatePost? post)
    {
        if (post == null)
            return null;

        return new PostDb(Guid.NewGuid(),
            post.Title,
            post.Salary,
            post.CompanyId
        );
    }

    [return: NotNullIfNotNull(nameof(post))]
    public static PostDb? Convert(BasePost? post)
    {
        if (post == null)
            return null;

        return new PostDb(post.Id,
            post.Title,
            post.Salary,
            post.CompanyId);
    }

    [return: NotNullIfNotNull(nameof(post))]
    public static BasePost? Convert(PostDb? post)
    {
        if (post == null)
            return null;

        return new BasePost(post.Id,
            post.Title,
            post.Salary,
            post.CompanyId);
    }
}