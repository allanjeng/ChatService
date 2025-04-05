using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MessageGeneratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// Authentication endpoints
app.MapPost("/api/auth/login", async (HttpRequest request, IAuthService authService) =>
    await Login(request, authService))
    .WithName("Login")
    .WithDescription("Authenticates a user or creates a new user if they don't exist")
    .WithSummary("User Authentication");

// Message endpoints
app.MapGet("/messages", async (ChatDbContext db) =>
    await GetMessages(db))
    .WithName("GetRecentMessages")
    .WithDescription("Retrieves the 50 most recent messages")
    .WithSummary("Get Recent Messages");

app.MapGet("/messages/all", async (ChatDbContext db) =>
    await GetAllMessages(db))
    .WithName("GetAllMessages")
    .WithDescription("Retrieves all messages from the database")
    .WithSummary("Get All Messages");

app.MapPost("/api/messages/generate", async (MessageGeneratorService generator, ChatDbContext db) =>
    await GenerateSampleMessages(generator, db))
    .WithName("GenerateSampleMessages")
    .WithDescription("Generates 50 sample messages with sequential numbering and current timestamps")
    .WithSummary("Generate Sample Messages");

// SignalR hub endpoint
app.MapHub<ChatHub>("/chathub");

// Default route
app.MapGet("/", async context =>
{
    try
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"An error occurred: {ex.Message}");
    }
});

app.Run();

// Static methods for endpoint handlers
static async Task<IResult> Login(HttpRequest request, IAuthService authService)
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var username = await reader.ReadToEndAsync();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.BadRequest("Username cannot be empty");
        }

        var user = await authService.AuthenticateAsync(username) ?? 
                  await authService.RegisterAsync(username);
        
        return Results.Ok(new { userId = user.Id, username = user.Username });
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred during authentication: {ex.Message}");
    }
}

static async Task<IResult> GetMessages(ChatDbContext db)
{
    try
    {
        var messages = await db.Messages
            .Include(m => m.User)
            .OrderByDescending(m => m.Timestamp)
            .Take(50)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
        return Results.Ok(messages);
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred while fetching messages: {ex.Message}");
    }
}

static async Task<IResult> GetAllMessages(ChatDbContext db)
{
    try
    {
        var messages = await db.Messages
            .Include(m => m.User)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
        return Results.Ok(messages);
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred while fetching all messages: {ex.Message}");
    }
}

static async Task<IResult> GenerateSampleMessages(MessageGeneratorService generator, ChatDbContext db)
{
    try
    {
        var messages = generator.GenerateMessages();
        await db.Messages.AddRangeAsync(messages);
        await db.SaveChangesAsync();
        return Results.Ok(new { count = messages.Count });
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred while generating messages: {ex.Message}");
    }
}