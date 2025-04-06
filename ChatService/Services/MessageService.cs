using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatService.Services;

/// <summary>
/// Service for handling message operations including retrieval and caching.
/// </summary>
public class MessageService(
    ChatDbContext dbContext,
    IMemoryCache cache,
    ILogger<MessageService> logger,
    IConfiguration configuration) : IMessageService
{
    private const string RecentMessagesCacheKey = "recent_messages";
    private const int DefaultMessageLimit = 50;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(
        configuration.GetValue<int>("CacheSettings:MessageCacheDuration", 5));

    /// <summary>
    /// Retrieves the most recent messages, up to the specified limit.
    /// </summary>
    public async Task<List<Message>> GetRecentMessagesAsync(int limit = DefaultMessageLimit)
    {
        try
        {
            if (cache.TryGetValue(RecentMessagesCacheKey, out List<Message>? cachedMessages))
            {
                logger.LogInformation("Retrieved {Count} messages from cache", cachedMessages?.Count ?? 0);
                return cachedMessages ?? new List<Message>();
            }

            var messages = await dbContext.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cacheDuration)
                .SetPriority(CacheItemPriority.High);

            cache.Set(RecentMessagesCacheKey, messages, cacheOptions);
            logger.LogInformation("Cached {Count} messages for {Duration} minutes", 
                messages.Count, _cacheDuration.TotalMinutes);

            return messages;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent messages");
            throw;
        }
    }

    /// <summary>
    /// Adds a new message to the system and invalidates the cache.
    /// </summary>
    public async Task<Message> AddMessageAsync(Message message)
    {
        try
        {
            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync();

            cache.Remove(RecentMessagesCacheKey);
            logger.LogInformation("Cache invalidated for new message from user {UserId}", message.UserId);

            return message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding message for user {UserId}", message.UserId);
            throw;
        }
    }

    /// <summary>
    /// Warms up the cache by preloading recent messages.
    /// </summary>
    public async Task WarmCacheAsync()
    {
        try
        {
            logger.LogInformation("Starting cache warm-up");
            await GetRecentMessagesAsync();
            logger.LogInformation("Cache warm-up completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cache warm-up");
        }
    }
} 