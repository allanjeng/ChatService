using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace ChatService.Hubs;

/// <summary>
/// SignalR hub for handling real-time chat functionality.
/// </summary>
public class ChatHub : Hub
{
    private readonly ChatDbContext _dbContext;
    private readonly ILogger<ChatHub> _logger;
    private readonly MessageService _messageService;
    private const int MaxMessageLength = 1000;

    public ChatHub(
        ChatDbContext dbContext,
        ILogger<ChatHub> logger,
        MessageService messageService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _messageService = messageService;
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <exception cref="HubException">Thrown when the user is not found or message is invalid.</exception>
    public async Task SendMessage(string message, int userId)
    {
        try
        {
            // Validate message
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new HubException("Message cannot be empty");
            }

            if (message.Length > MaxMessageLength)
            {
                throw new HubException($"Message exceeds maximum length of {MaxMessageLength} characters");
            }

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when attempting to send message", userId);
                throw new HubException("User not found");
            }

            var msg = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            // Use MessageService to add the message
            await _messageService.AddMessageAsync(msg);

            _logger.LogInformation("Message sent by user {Username}", user.Username);
            await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message for user {UserId}", userId);
            throw new HubException("An error occurred while sending the message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        // Send recent messages to the newly connected client
        try
        {
            var recentMessages = await _messageService.GetRecentMessagesAsync();
            await Clients.Caller.SendAsync("ReceiveRecentMessages", recentMessages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recent messages to client {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}