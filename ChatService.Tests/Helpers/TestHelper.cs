using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChatService.Tests.Helpers;

public static class TestHelper
{
    public static DbContextOptions<ChatDbContext> CreateInMemoryDbContextOptions()
    {
        return new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;
    }

    public static Mock<IConfiguration> CreateMockConfiguration(int cacheDuration = 5)
    {
        var mockConfiguration = new Mock<IConfiguration>();
        var mockConfigSection = new Mock<IConfigurationSection>();
        mockConfigSection.Setup(x => x.Value).Returns(cacheDuration.ToString());
        mockConfiguration.Setup(x => x.GetSection("CacheSettings:MessageCacheDuration"))
            .Returns(mockConfigSection.Object);
        return mockConfiguration;
    }

    public static Mock<IMemoryCache> CreateMockMemoryCache()
    {
        var mockCache = new Mock<IMemoryCache>();
        var cacheEntry = new Mock<ICacheEntry>();
        mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(cacheEntry.Object);
        return mockCache;
    }

    public static async Task<User> CreateTestUserAsync(ChatDbContext context, string username = "testuser")
    {
        var user = new User { Username = username };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public static async Task<List<Message>> CreateTestMessagesAsync(ChatDbContext context, User user, int count = 2)
    {
        var messages = new List<Message>();
        for (int i = 1; i <= count; i++)
        {
            messages.Add(new Message
            {
                Content = $"Message {i}",
                UserId = user.Id
            });
        }
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        return messages;
    }

    public static Mock<ILogger<T>> CreateMockLogger<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }
} 