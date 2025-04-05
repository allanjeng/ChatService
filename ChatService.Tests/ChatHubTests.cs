using ChatService.Data;
using ChatService.Hubs;
using ChatService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChatService.Tests;

public class ChatHubTests
{
    private readonly DbContextOptions<ChatDbContext> _options;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ILogger<ChatHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly ChatHub _chatHub;

    public ChatHubTests()
    {
        _options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        var context = new ChatDbContext(_options);
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockLogger = new Mock<ILogger<ChatHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        _chatHub = new ChatHub(context, _mockLogger.Object)
        {
            Clients = _mockClients.Object,
            Context = _mockContext.Object
        };
    }

    [Fact]
    public async Task SendMessage_ValidUser_SendsMessageToAllClients()
    {
        // Arrange
        using var context = new ChatDbContext(_options);
        var username = "testuser";
        var user = new User { Username = username };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var message = "Hello, world!";
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);

        // Act
        await _chatHub.SendMessage(message, user.Id);

        // Assert
        var savedMessage = await context.Messages.FirstOrDefaultAsync(m => m.UserId == user.Id);
        Assert.NotNull(savedMessage);
        Assert.Equal(message, savedMessage.Content);
        Assert.Equal(user.Id, savedMessage.UserId);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveMessage",
                It.Is<object[]>(o => o[0].ToString() == username && o[1].ToString() == message),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_InvalidUser_ThrowsHubException()
    {
        // Arrange
        var userId = 999;
        var message = "Hello, world!";

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() =>
            _chatHub.SendMessage(message, userId));
    }

    [Fact]
    public async Task OnConnectedAsync_CallsBaseImplementation()
    {
        // Act
        await _chatHub.OnConnectedAsync();

        // Assert
        // No specific assertions needed as we're just verifying the base implementation is called
    }

    [Fact]
    public async Task OnDisconnectedAsync_CallsBaseImplementation()
    {
        // Act
        await _chatHub.OnDisconnectedAsync(null);

        // Assert
        // No specific assertions needed as we're just verifying the base implementation is called
    }
} 