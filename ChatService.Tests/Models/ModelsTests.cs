using ChatService.Models;

namespace ChatService.Tests.Models;

public class ModelsTests
{
    [Fact]
    public void User_Properties_SetCorrectly()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Username = "testuser"
        };

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public void Message_Properties_SetCorrectly()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var user = new User { Id = 1, Username = "testuser" };

        // Act
        var message = new Message
        {
            Id = 1,
            UserId = user.Id,
            Content = "Hello, world!",
            Timestamp = timestamp,
            User = user
        };

        // Assert
        Assert.Equal(1, message.Id);
        Assert.Equal(user.Id, message.UserId);
        Assert.Equal("Hello, world!", message.Content);
        Assert.Equal(timestamp, message.Timestamp);
        Assert.NotNull(message.User);
        Assert.Equal(user.Id, message.User.Id);
        Assert.Equal(user.Username, message.User.Username);
    }

    [Fact]
    public void Message_Timestamp_DefaultsToUtcNow()
    {
        // Arrange & Act
        var message = new Message
        {
            Id = 1,
            UserId = 1,
            Content = "Test message"
        };

        // Assert
        Assert.True(message.Timestamp <= DateTime.UtcNow);
        Assert.True(message.Timestamp >= DateTime.UtcNow.AddSeconds(-1));
    }
}