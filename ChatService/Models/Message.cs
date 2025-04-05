using System;

namespace ChatService.Models;

/// <summary>
/// Represents a chat message in the system.
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier for the message.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the user who sent the message.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Content of the message.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Timestamp when the message was sent (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property to the user who sent the message.
    /// </summary>
    public User? User { get; set; }
}