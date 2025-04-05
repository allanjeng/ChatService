using ChatService.Data;
using ChatService.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _dbContext;

    public ChatHub(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SendMessage(string message, int userId)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new HubException("User not found");
            }

            var msg = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            _dbContext.Messages.Add(msg);
            await _dbContext.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", user.Username, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            throw;
        }
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}