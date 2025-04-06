using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatService.Services;

/// <summary>
/// Service for handling message operations including retrieval and caching.
/// This service manages the lifecycle of chat messages, including storage,
/// retrieval, and caching operations.
/// </summary>
/// <param name="dbContext">The database context for message persistence</param>
/// <param name="cache">The memory cache for temporary message storage</param>
/// <param name="logger">Logger instance for tracking service operations</param>
/// <param name="configuration">Application configuration for cache settings</param>
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
    /// Messages are first attempted to be retrieved from cache, falling back to
    /// database retrieval if not found in cache.
    /// </summary>
    /// <param name="limit">Maximum number of messages to retrieve (default: 50)</param>
    /// <returns>A list of recent messages with user information</returns>
    /// <exception cref="Exception">Thrown when there's an error retrieving messages</exception>
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
    /// <param name="message">The message to add to the system</param>
    /// <returns>The added message with updated ID</returns>
    /// <exception cref="Exception">Thrown when there's an error adding the message</exception>
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
    /// This method is typically called during application startup to improve
    /// initial response times for new connections.
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