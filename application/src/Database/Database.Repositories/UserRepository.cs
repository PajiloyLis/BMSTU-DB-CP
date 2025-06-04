using Database.Context;
using Database.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models.User;
using Project.Core.Repositories;

namespace Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ProjectDbContext context, ILogger<UserRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BaseUser> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == email);


            if (user is null)
            {
                _logger.LogWarning($"User with such email {email} not found");
                throw new UserNotFoundException($"User with email {email} not found");
            }

            return UserConverter.Convert(user);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Error getting user with email {email}");
            throw;
        }
    }

    public async Task DeleteUserByIdAsync(string email)
    {
        try
        {
            var user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                _logger.LogWarning($"User with email {email} not found");
                throw new UserNotFoundException($"User with email {email} not found");
            }

            _context.UserDb.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, $"Error deleting user with email {email}");
            throw;
        }
    }

    public async Task UpdateUserByIdAsync(string emailOld, string passwordOld, string? passwordHash, string? email)
    {
        try
        {
            var user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == emailOld);
            if (user is null)
            {
                _logger.LogWarning($"User with email {email} not found");
                throw new UserNotFoundException($"User with email {email} not found");
            }
            user = await _context.UserDb.FirstOrDefaultAsync(u => u.Email == email);
            if (user is not null)
            {
                _logger.LogWarning($"User with email {email} already exists.");
                throw new UserAlreadyExistsException($"User with email {email} already exists.");
            }
                
            user.Password = passwordHash ?? passwordOld;
            user.Email = email ?? emailOld;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error updating user with email {email}.");
            throw;
        }
    }

    public async Task<BaseUser> AddUserAsync(BaseUser user)
    {
        try
        {
            var checkUser = await _context.UserDb.FirstOrDefaultAsync(u => u.Email==user.Email);
            if (checkUser is not null)
            {
                _logger.LogWarning($"User with email {user.Email} already exists");
                throw new UserNotFoundException($"User with email {user.Email} already exists.");
            }
            var companyEmail = await _context.CompanyDb.Select(e => e.Email).ToListAsync();
            user.Role = companyEmail.Contains(user.Email) ? "admin" : "employee";
            await _context.UserDb.AddAsync(UserConverter.Convert(user));
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error adding user with email {user.Email}.");
            throw;
        }
    }

    public async Task<Guid> GetCurrentUserIdAsync(string email)
    {
        try
        {
            var employee = await _context.EmployeeDb.FirstOrDefaultAsync(e => e.Email == email);
            if (employee is null)
            {
                _logger.LogWarning($"User with email {email} not found");
                throw new UserNotFoundException($"User with email {email} not found");
            }

            return employee.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}