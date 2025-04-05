using ChatService.Data;
using ChatService.Models;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;

/// <summary>
/// SignalR hub for handling real-time chat functionality.
/// </summary>
public class ChatHub(ChatDbContext dbContext, ILogger<ChatHub> logger) : Hub
{

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="userId">The ID of the user sending the message.</param>
    /// <exception cref="HubException">Thrown when the user is not found.</exception>
    public async Task SendMessage(string message, int userId)
    {
        try
        {
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

            dbContext.Messages.Add(msg);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Message sent by user {Username}", user.Username);
            await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message for user {UserId}", userId);
            throw;
        }
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}