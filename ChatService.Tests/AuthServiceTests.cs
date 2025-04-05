using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ChatService.Tests;

public class AuthServiceTests
{
    private readonly DbContextOptions<ChatDbContext> _options;
    private readonly AuthService _authService;
    private readonly Mock<DbSet<User>> _mockUsersSet;

    public AuthServiceTests()
    {
        _options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        var context = new ChatDbContext(_options);
        _mockUsersSet = new Mock<DbSet<User>>();
        _authService = new AuthService(context);
    }

    [Fact]
    public async Task AuthenticateAsync_UserExists_ReturnsUser()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var username = "testuser";
        var user = new User { Username = username };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var username = "nonexistentuser";

        // Act
        var result = await _authService.AuthenticateAsync(username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_ValidUsername_CreatesUser()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var username = "newuser";

        // Act
        var result = await _authService.RegisterAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(savedUser);
        Assert.Equal(username, savedUser.Username);
    }
} 