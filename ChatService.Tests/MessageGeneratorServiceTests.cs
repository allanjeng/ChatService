using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChatService.Tests;

public class MessageServiceTests
{
    private readonly DbContextOptions<ChatDbContext> _options;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<MessageService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        var context = new ChatDbContext(_options);
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<MessageService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        var mockConfigSection = new Mock<IConfigurationSection>();
        mockConfigSection.Setup(x => x.Value).Returns("5");
        _mockConfiguration.Setup(x => x.GetSection("CacheSettings:MessageCacheDuration"))
            .Returns(mockConfigSection.Object);

        // Setup cache mock
        var cacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry.Object);

        _messageService = new MessageService(
            context,
            _mockCache.Object,
            _mockLogger.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetRecentMessagesAsync_NoCache_ReturnsMessagesFromDatabase()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var user = new User { Username = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var messages = new List<Message>
        {
            new() { Content = "Message 1", UserId = user.Id },
            new() { Content = "Message 2", UserId = user.Id }
        };
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();

        _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        // Act
        var result = await _messageService.GetRecentMessagesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Content == "Message 1");
        Assert.Contains(result, m => m.Content == "Message 2");
    }

    [Fact]
    public async Task GetRecentMessagesAsync_WithCache_ReturnsCachedMessages()
    {
        // Arrange
        var cachedMessages = new List<Message>
        {
            new Message { Content = "Cached Message 1", UserId = 1 },
            new Message { Content = "Cached Message 2", UserId = 1 }
        };

        object cachedValue = cachedMessages;
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        // Act
        var result = await _messageService.GetRecentMessagesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Content == "Cached Message 1");
        Assert.Contains(result, m => m.Content == "Cached Message 2");
    }

    [Fact]
    public async Task AddMessageAsync_ValidMessage_AddsToDatabase()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var user = new User { Username = "testuser" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var message = new Message
        {
            Content = "New message",
            UserId = user.Id
        };

        // Act
        var result = await _messageService.AddMessageAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("New message", result.Content);
        Assert.Equal(user.Id, result.UserId);

        var savedMessage = await context.Messages.FindAsync(result.Id);
        Assert.NotNull(savedMessage);
        Assert.Equal("New message", savedMessage.Content);
    }

    [Fact]
    public async Task WarmCacheAsync_NoException_CompletesSuccessfully()
    {
        // Arrange
        _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny))
            .Returns(false);

        // Act & Assert
        await _messageService.WarmCacheAsync();
        // No exception means success
    }
} 