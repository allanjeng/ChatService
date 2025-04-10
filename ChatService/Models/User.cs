namespace ChatService.Models;

/// <summary>
/// Represents a user in the chat system.
/// This class defines the basic user information required for the chat application.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// This ID is automatically generated by the database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username of the user.
    /// This is the display name used in the chat interface.
    /// </summary>
    public required string Username { get; set; }
    // Add any additional user properties as needed
}
