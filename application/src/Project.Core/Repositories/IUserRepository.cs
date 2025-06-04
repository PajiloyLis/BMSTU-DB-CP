using Project.Core.Models.User;

namespace Project.Core.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>The user with the specified email.</returns>
    Task<BaseUser> GetUserByEmailAsync(string email);

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="email"></param>
    Task DeleteUserByIdAsync(string email);

    /// <summary>
    /// Updates the details of a user identified by their unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be updated.</param>
    /// <param name="login">The new login of the user (optional).</param>
    /// <param name="phone">The new phone number of the user (optional).</param>
    /// <param name="passwordHash">The new hashed password of the user (optional).</param>
    /// <param name="email">The new email address of the user (optional).</param>
    /// <param name="agreeWithTerms">Indicates whether the user has agreed to the terms of use (optional).</param>
    /// <param name="permissions">The updated list of permissions for the user (optional).</param>
    /// <param name="lastActivity">The updated date and time of the user's last activity (optional).</param>
    /// <returns>The updated user.</returns>
    Task UpdateUserByIdAsync(string emailOld, string passwordOld,
        string? passwordHash,
        string? email);

    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user object to be added.</param>
    /// <returns>The added user.</returns>
    Task<BaseUser> AddUserAsync(BaseUser user);

    Task<Guid> GetCurrentUserIdAsync(string email);
}