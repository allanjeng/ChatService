using ChatService.Data;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ChatDbContext _dbContext;
    private readonly MessageGeneratorService _messageGenerator;

    public MessagesController(ChatDbContext dbContext, MessageGeneratorService messageGenerator)
    {
        _dbContext = dbContext;
        _messageGenerator = messageGenerator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentMessages()
    {
        try
        {
            var messages = await _dbContext.Messages
                .Include(m => m.User)
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return Problem($"An error occurred while fetching messages: {ex.Message}");
        }
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMessages()
    {
        try
        {
            var messages = await _dbContext.Messages
                .Include(m => m.User)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return Problem($"An error occurred while fetching all messages: {ex.Message}");
        }
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateSampleMessages()
    {
        try
        {
            var messages = _messageGenerator.GenerateMessages();
            await _dbContext.Messages.AddRangeAsync(messages);
            await _dbContext.SaveChangesAsync();
            return Ok(new { count = messages.Count });
        }
        catch (Exception ex)
        {
            return Problem($"An error occurred while generating messages: {ex.Message}");
        }
    }
} 