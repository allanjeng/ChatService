using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username);
    Task<User> RegisterAsync(string username);
}

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