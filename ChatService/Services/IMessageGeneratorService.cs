using ChatService.Models;

namespace ChatService.Services;

/// <summary>
/// Interface for generating sample messages for testing and development purposes.
/// </summary>
public interface IMessageGeneratorService
{
    /// <summary>
    /// Generates a specified number of sample messages with sequential numbering and current timestamps.
    /// </summary>
    /// <param name="count">Number of messages to generate</param>
    /// <param name="userId">Optional user ID to associate with the messages</param>
    /// <returns>List of generated messages</returns>
    List<Message> GenerateMessages(int count = 50, int userId = 1);
}