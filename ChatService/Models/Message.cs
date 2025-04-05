using System;

namespace ChatService.Models;

public class Message
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property (optional)
    public User User { get; set; }
}
