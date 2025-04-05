using ChatService.Data;
using ChatService.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _dbContext;

    public ChatHub(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendMessage(string user, string message)
    {
        // Create and persist the new message.
        // For this minimal demo, we'll assume a default user with ID 1.
        // In a full app, you'd look up the user by name or use authentication.
        var msg = new Message
        {
            Content = message,
            Timestamp = DateTime.UtcNow,
            UserId = 1
        };

        _dbContext.Messages.Add(msg);
        await _dbContext.SaveChangesAsync();

        // Broadcast the message to all connected clients.
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    
}