using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Services;

/// <summary>
/// Interface for authentication-related operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user by their username.
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <returns>The authenticated user, or null if not found.</returns>
    Task<User?> AuthenticateAsync(string username);

    /// <summary>
    /// Registers a new user with the given username.
    /// </summary>
    /// <param name="username">The username for the new user.</param>
    /// <returns>The newly created user.</returns>
    Task<User> RegisterAsync(string username);
}

/// <summary>
/// Implementation of the authentication service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ChatDbContext _dbContext;

    public AuthService(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> AuthenticateAsync(string username)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> RegisterAsync(string username)
    {
        var user = new User { Username = username };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
} 