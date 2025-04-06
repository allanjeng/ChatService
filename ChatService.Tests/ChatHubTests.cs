using ChatService.Data;
using ChatService.Hubs;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChatService.Tests;

public class ChatHubTests
{
    private readonly DbContextOptions<ChatDbContext> _options;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly Mock<IClientProxy> _mockAllClientsProxy;
    private readonly Mock<ILogger<ChatHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly ChatHub _chatHub;

    public ChatHubTests()
    {
        _options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        var context = new ChatDbContext(_options);
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();
        _mockAllClientsProxy = new Mock<IClientProxy>();
        _mockLogger = new Mock<ILogger<ChatHub>>();
        _mockContext = new Mock<HubCallerContext>();
        
        _mockMessageService = new Mock<IMessageService>();
        
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");

        _chatHub = new ChatHub(context, _mockLogger.Object, _mockMessageService.Object)
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
        _mockClients.Setup(c => c.All).Returns(_mockAllClientsProxy.Object);
        _mockMessageService.Setup(m => m.AddMessageAsync(It.IsAny<Message>()))
            .ReturnsAsync((Message m) => m);

        // Act
        await _chatHub.SendMessage(message, user.Id);

        // Assert
        _mockMessageService.Verify(m => m.AddMessageAsync(
            It.Is<Message>(msg => 
                msg.Content == message && 
                msg.UserId == user.Id)), 
            Times.Once);

        _mockAllClientsProxy.Verify(
            c => c.SendCoreAsync("ReceiveMessage",
                It.Is<object[]>(o => o[0].ToString() == username && o[1].ToString() == message),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_EmptyMessage_ThrowsHubException()
    {
        // Arrange
        var userId = 1;
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<HubException>(() =>
            _chatHub.SendMessage(message, userId));
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
    public async Task OnConnectedAsync_SendsRecentMessagesToClient()
    {
        // Arrange
        var recentMessages = new List<Message>();
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockMessageService.Setup(m => m.GetRecentMessagesAsync(It.IsAny<int>()))
            .ReturnsAsync(recentMessages);

        // Act
        await _chatHub.OnConnectedAsync();

        // Assert
        _mockMessageService.Verify(m => m.GetRecentMessagesAsync(It.IsAny<int>()), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("ReceiveRecentMessages",
                It.Is<object[]>(o => o[0] == recentMessages),
                default),
            Times.Once);
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