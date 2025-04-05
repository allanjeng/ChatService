namespace ChatService.Models;

/// <summary>
/// Represents a login request.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username to authenticate with.
    /// </summary>
    public required string Username { get; set; }
} 