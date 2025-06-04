using Project.Core.Models.User;

namespace Project.Core.Services;

public interface IAuthorizationService
{
    /// <summary>
    /// Authenticates a user based on email and password.
    /// </summary>
    /// <param name="email">The user's phone number.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A task representing the asynchronous operation, containing the authorization data.</returns>
    public Task<AuthorizationData> LoginAsync(string email, string password);

    /// <summary>
    /// Registers a new user with the provided details.
    /// </summary>
    /// <param name="id">Unique identifier for the user.</param>
    /// <param name="email">The user's phone number.</param>
    /// <param name="login">The user's login.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="agreeWithTerms">Indicates whether the user has agreed to the terms of use.</param>
    /// <param name="permissions">An array of permissions granted to the user.</param>
    /// <returns>A task representing the asynchronous operation, containing the authorization data.</returns>
    public Task<AuthorizationData> RegisterAsync(string password, 
        string email);

    Task<Guid> GetCurrentUserIdAsync(string email);
}