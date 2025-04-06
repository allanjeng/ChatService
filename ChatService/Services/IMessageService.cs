using ChatService.Models;

namespace ChatService.Services;

/// <summary>
/// Interface for message-related operations including retrieval and caching.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Retrieves the most recent messages, up to the specified limit.
    /// </summary>
    /// <param name="limit">Maximum number of messages to retrieve (default: 50)</param>
    /// <returns>List of recent messages with user information</returns>
    Task<List<Message>> GetRecentMessagesAsync(int limit = 50);

    /// <summary>
    /// Adds a new message to the system and invalidates the cache.
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <returns>The added message with updated ID</returns>
    Task<Message> AddMessageAsync(Message message);

    /// <summary>
    /// Warms up the cache by preloading recent messages.
    /// </summary>
    Task WarmCacheAsync();
} 