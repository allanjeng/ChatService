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
    Task<User?> AuthenticateAsync(string username);

    /// <summary>
    /// Registers a new user with the given username.
    /// </summary>
    Task<User> RegisterAsync(string username);
}

/// <summary>
/// Implementation of the authentication service.
/// </summary>
public class AuthService(ChatDbContext dbContext) : IAuthService
{
    public async Task<User?> AuthenticateAsync(string username)
    {
        return await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> RegisterAsync(string username)
    {
        var user = new User { Username = username };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }
}