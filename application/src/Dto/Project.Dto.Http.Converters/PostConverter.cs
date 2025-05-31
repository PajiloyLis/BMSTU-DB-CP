using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Post;
using Project.Dto.Http.Post;

namespace Project.Dto.Http.Converters;

public static class PostConverter
{
    [return: NotNullIfNotNull(nameof(post))]
    public static PostDto? Convert(BasePost? post)
    {
        if (post is null)
            return null;

        return new PostDto(post.Id,
            post.Title,
            post.Salary,
            post.CompanyId
        );
    }
}