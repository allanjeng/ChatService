using ChatService.Models;

namespace ChatService.Services;

/// <summary>
/// Service for generating sample messages for testing and development purposes.
/// </summary>
public class MessageGeneratorService : IMessageGeneratorService
{
    /// <summary>
    /// Generates a specified number of sample messages with sequential numbering and current timestamps.
    /// </summary>
    /// <param name="count">Number of messages to generate</param>
    /// <param name="userId">Optional user ID to associate with the messages</param>
    /// <returns>List of generated messages</returns>
    public List<Message> GenerateMessages(int count = 50, int userId = 1)
    {
        var messages = new List<Message>();
        var baseTime = DateTime.UtcNow.AddSeconds(-count); // Start from count seconds ago

        for (int i = 1; i <= count; i++)
        {
            messages.Add(new Message
            {
                UserId = userId,
                Content = $"Sample message #{i}",
                Timestamp = baseTime.AddSeconds(i) // Each message is 1 second after the previous
            });
        }

        return messages;
    }
} 