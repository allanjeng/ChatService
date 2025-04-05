using ChatService.Models;
using ChatService.Services;
using Xunit;

namespace ChatService.Tests;

public class MessageGeneratorServiceTests
{
    private readonly MessageGeneratorService _service;

    public MessageGeneratorServiceTests()
    {
        _service = new MessageGeneratorService();
    }

    [Fact]
    public void GenerateMessages_DefaultCount_Returns50Messages()
    {
        // Act
        var messages = _service.GenerateMessages();

        // Assert
        Assert.Equal(50, messages.Count);
    }

    [Fact]
    public void GenerateMessages_CustomCount_ReturnsCorrectNumberOfMessages()
    {
        // Arrange
        const int count = 10;

        // Act
        var messages = _service.GenerateMessages(count);

        // Assert
        Assert.Equal(count, messages.Count);
    }

    [Fact]
    public void GenerateMessages_CustomUserId_AssignsCorrectUserId()
    {
        // Arrange
        const int userId = 42;

        // Act
        var messages = _service.GenerateMessages(5, userId);

        // Assert
        Assert.All(messages, m => Assert.Equal(userId, m.UserId));
    }

    [Fact]
    public void GenerateMessages_SequentialContent_ContainsCorrectMessageNumbers()
    {
        // Act
        var messages = _service.GenerateMessages(5);

        // Assert
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal($"Sample message #{i + 1}", messages[i].Content);
        }
    }

    [Fact]
    public void GenerateMessages_Timestamps_AreSequential()
    {
        // Act
        var messages = _service.GenerateMessages(5);

        // Assert
        for (int i = 1; i < messages.Count; i++)
        {
            Assert.True(messages[i].Timestamp > messages[i - 1].Timestamp);
        }
    }

    [Fact]
    public void GenerateMessages_Timestamps_AreWithinExpectedRange()
    {
        // Act
        var messages = _service.GenerateMessages(5);
        var now = DateTime.UtcNow;

        // Assert
        foreach (var message in messages)
        {
            Assert.True(message.Timestamp <= now);
            Assert.True(message.Timestamp >= now.AddSeconds(-5));
        }
    }
} 