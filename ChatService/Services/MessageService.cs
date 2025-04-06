using ChatService.Data;
using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ChatService.Services;

/// <summary>
/// Service for handling message operations including retrieval and caching.
/// </summary>
public class MessageService
{
    private const string RecentMessagesCacheKey = "recent_messages";
    private const int DefaultMessageLimit = 50;
    private readonly ChatDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MessageService> _logger;
    private readonly TimeSpan _cacheDuration;

    public MessageService(
        ChatDbContext dbContext,
        IMemoryCache cache,
        ILogger<MessageService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
        _cacheDuration = TimeSpan.FromMinutes(
            configuration.GetValue<int>("CacheSettings:MessageCacheDuration", 5));
    }

    /// <summary>
    /// Retrieves the most recent messages, up to the specified limit.
    /// </summary>
    /// <param name="limit">Maximum number of messages to retrieve (default: 50)</param>
    /// <returns>List of recent messages with user information</returns>
    public async Task<List<Message>> GetRecentMessagesAsync(int limit = DefaultMessageLimit)
    {
        try
        {
            // Try to get messages from cache first
            if (_cache.TryGetValue(RecentMessagesCacheKey, out List<Message>? cachedMessages))
            {
                _logger.LogInformation("Retrieved {Count} messages from cache", cachedMessages?.Count ?? 0);
                return cachedMessages ?? new List<Message>();
            }

            // If not in cache, get from database
            var messages = await _dbContext.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .OrderBy(m => m.Timestamp) // Reorder for chronological display
                .ToListAsync();

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(RecentMessagesCacheKey, messages, cacheOptions);
            _logger.LogInformation("Cached {Count} messages for {Duration} minutes", 
                messages.Count, _cacheDuration.TotalMinutes);

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent messages");
            throw;
        }
    }

    /// <summary>
    /// Adds a new message to the system and invalidates the cache.
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <returns>The added message with updated ID</returns>
    public async Task<Message> AddMessageAsync(Message message)
    {
        try
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // Invalidate the cache
            _cache.Remove(RecentMessagesCacheKey);
            _logger.LogInformation("Cache invalidated for new message from user {UserId}", message.UserId);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message for user {UserId}", message.UserId);
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
            _logger.LogInformation("Starting cache warm-up");
            await GetRecentMessagesAsync();
            _logger.LogInformation("Cache warm-up completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warm-up");
            // Don't throw - this is a non-critical operation
        }
    }
} 