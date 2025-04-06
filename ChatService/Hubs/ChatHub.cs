using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

/// <summary>
/// SignalR hub for handling real-time chat functionality.
/// This hub manages real-time communication between clients, including message broadcasting,
/// connection management, and message history retrieval.
/// </summary>
/// <param name="dbContext">The database context for accessing chat data</param>
/// <param name="logger">Logger instance for tracking hub operations</param>
/// <param name="messageService">Service for managing message operations</param>
public class ChatHub(
    ChatDbContext dbContext,
    ILogger<ChatHub> logger,
    IMessageService messageService) : Hub
{
    private const int MaxMessageLength = 1000;

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message content to be sent</param>
    /// <param name="userId">The ID of the user sending the message</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="HubException">
    /// Thrown when:
    /// - The message is empty or whitespace
    /// - The message exceeds maximum length
    /// - The user is not found
    /// - An unexpected error occurs
    /// </exception>
    public async Task SendMessage(string message, int userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new HubException("Message cannot be empty");
            }

            if (message.Length > MaxMessageLength)
            {
                throw new HubException($"Message exceeds maximum length of {MaxMessageLength} characters");
            }

            var user = await dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                logger.LogWarning("User {UserId} not found when attempting to send message", userId);
                throw new HubException("User not found");
            }

            var msg = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            await messageService.AddMessageAsync(msg);

            logger.LogInformation("Message sent by user {Username}", user.Username);
            await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message for user {UserId}", userId);
            throw new HubException("An error occurred while sending the message");
        }
    }

    /// <summary>
    /// Called when a new client connects to the hub.
    /// Sends recent messages to the newly connected client.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);

        try
        {
            var recentMessages = await messageService.GetRecentMessagesAsync();
            await Clients.Caller.SendAsync("ReceiveRecentMessages", recentMessages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending recent messages to client {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect, if any</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}