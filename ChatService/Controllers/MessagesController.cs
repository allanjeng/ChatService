using ChatService.Configuration;
using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ChatDbContext _dbContext;
    private readonly IMessageGeneratorService _messageGenerator;
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<MessagesController> _logger;
    private const string RecentMessagesCacheKey = "recent_messages";
    private const int MaxRecentMessages = 50;

    public MessagesController(
        ChatDbContext dbContext,
        IMessageGeneratorService messageGenerator,
        IMemoryCache cache,
        IOptions<CacheSettings> cacheSettings,
        ILogger<MessagesController> logger)
    {
        _dbContext = dbContext;
        _messageGenerator = messageGenerator;
        _cache = cache;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentMessages()
    {
        try
        {
            _logger.LogInformation("Attempting to get recent messages from cache");
            if (_cache.TryGetValue(RecentMessagesCacheKey, out List<Message>? cachedMessages))
            {
                _logger.LogInformation("Cache hit for recent messages");
                return Ok(cachedMessages);
            }

            _logger.LogInformation("Cache miss for recent messages, fetching from database");
            var messages = await _dbContext.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.Timestamp)
                .Take(MaxRecentMessages)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var cacheDuration = _cacheSettings.GetMessageCacheDuration();
            _logger.LogInformation("Setting cache for recent messages with duration: {Duration}", cacheDuration);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheDuration)
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1); // We only cache one list of 50 messages

            _cache.Set(RecentMessagesCacheKey, messages, cacheOptions);
            _logger.LogInformation("Successfully cached recent messages");

            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching recent messages");
            return Problem($"An error occurred while fetching messages: {ex.Message}");
        }
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateSampleMessages()
    {
        try
        {
            _logger.LogInformation("Generating sample messages");
            var messages = _messageGenerator.GenerateMessages();
            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Invalidating message cache");
            _cache.Remove(RecentMessagesCacheKey);

            return Ok(new { count = messages.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating messages");
            return Problem($"An error occurred while generating messages: {ex.Message}");
        }
    }
}