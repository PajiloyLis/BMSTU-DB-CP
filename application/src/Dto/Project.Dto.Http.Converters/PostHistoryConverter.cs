using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.PostHistory;
using Project.Dto.Http.Post;
using Project.Dto.Http.PostHistory;

namespace Project.Dto.Http.Converters;

public static class PostHistoryConverter
{
    [return: NotNullIfNotNull(nameof(post))]
    public static PostHistoryDto? Convert(BasePostHistory? post)
    {
        if (post is null)
            return null;

        return new PostHistoryDto(post.PostId,
            post.EmployeeId,
            post.StartDate,
            post.EndDate
        );
    }
}