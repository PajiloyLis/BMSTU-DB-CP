using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project.Core.Exceptions;
using Project.Core.Models.User;
using Project.Core.Repositories;
using Project.Core.Services;
using Project.Service.AuthorizationService.Configuration;

namespace Project.Services.AuthorizationService;

public class AuthorizationService : IAuthorizationService
{
    private readonly ILogger<AuthorizationService> _logger;
    private readonly IUserRepository _usersRepository;
    private readonly JwtConfiguration _jwtConfiguration;
    private readonly PasswordHashingConfiguration _hashingConfiguration;

    public AuthorizationService(IUserRepository userRepository,
        IOptions<JwtConfiguration> jwtConfiguration, IOptions<PasswordHashingConfiguration> hashingConfiguration,
        ILogger<AuthorizationService> logger)
    {
        _usersRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtConfiguration = jwtConfiguration.Value ?? throw new ArgumentNullException(nameof(jwtConfiguration));
        _hashingConfiguration = hashingConfiguration.Value ?? throw new ArgumentNullException(nameof(hashingConfiguration));
    }

    public async Task<AuthorizationData> RegisterAsync(
        string password,
        string email)
    {
        try
        {
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(password, salt);
            var user = await _usersRepository.AddUserAsync(new BaseUser(email, hashedPassword, salt, "admin"));

            var accessToken = GenerateJwtToken(email, user.Role);

            return new AuthorizationData(email, accessToken);
        }
        catch (UserAlreadyExistsException ex)
        {
            _logger.LogWarning(ex, $"User with email: {email} already exists");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering the user");
            throw;
        }
    }


    public async Task<AuthorizationData> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _usersRepository.GetUserByEmailAsync(email);
            if (user is null)
            {
                throw new UserNotFoundException($"User with email: {email} not found");
            }

            var hashedPassword = HashPassword(password, user.Salt);
            if (hashedPassword != user.Password)
            {
                throw new InvalidPasswordException($"Invalid password for email: {email}");
            }
            
            var accessToken = GenerateJwtToken(email, user.Role);
            
            return new AuthorizationData(email, accessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging in user with email: {email}");
            throw;
        }
    }
    
    private string GenerateSalt(int size = 64)
    {
        var salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return Convert.ToBase64String(salt);
    }

    private string HashPassword(string password, string salt)
    {
        int iterations = _hashingConfiguration.Iterations;
        int len = _hashingConfiguration.KeyLen;
        var saltBytes = Convert.FromBase64String(salt);
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, saltBytes, KeyDerivationPrf.HMACSHA512, iterations, len));
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    private string GenerateJwtToken(string email, string role)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.Email, email),
            new (ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecurityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer,
            audience: _jwtConfiguration.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(_jwtConfiguration.AccessTokenLifetimeMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<Guid> GetCurrentUserIdAsync(string email)
    {
        try
        {
            var id = await _usersRepository.GetCurrentUserIdAsync(email);
            return id;
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error getting user with email: {email}");
            throw;
        }
    }
}
