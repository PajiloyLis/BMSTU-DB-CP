using Project.Core.Models;
using Project.Core.Models.Post;

namespace Project.Core.Repositories;

public interface IPostRepository
{
    /// <summary>
    /// Добавляет новую должность
    /// </summary>
    /// <param name="post">Должность для добавления</param>
    /// <returns>Добавленная должность</returns>
    Task<BasePost> AddPostAsync(CreatePost post);

    /// <summary>
    /// Получает должность по ID
    /// </summary>
    /// <param name="postId">ID должности</param>
    /// <returns>Должность</returns>
    Task<BasePost> GetPostByIdAsync(Guid postId);

    /// <summary>
    /// Обновляет существующую должность
    /// </summary>
    /// <param name="post">Должность для обновления</param>
    /// <returns>Обновленная должность</returns>
    Task<BasePost> UpdatePostAsync(UpdatePost post);

    /// <summary>
    /// Получает список должностей с пагинацией
    /// </summary>
    /// <param name="companyId">ID компании</param>
    /// <param name="pageNumber">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <returns>Страница с должностями</returns>
    Task<PostPage> GetPostsAsync(Guid companyId, int pageNumber, int pageSize);

    /// <summary>
    /// Удаляет должность
    /// </summary>
    /// <param name="postId">ID должности</param>
    Task DeletePostAsync(Guid postId);
}