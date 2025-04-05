namespace ChatService.Models;

/// <summary>
/// Represents a user in the chat system.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username of the user.
    /// </summary>
    public required string Username { get; set; }
    // Add any additional user properties as needed
}
