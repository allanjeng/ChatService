using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DbContext with SQLite
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

// Add SignalR service
builder.Services.AddSignalR();

// For minimal API endpoints (if needed)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Apply any pending migrations and create the database (for demo purposes)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// Authentication endpoints
app.MapPost("/api/auth/login", async (HttpRequest request, IAuthService authService) =>
{
    using var reader = new StreamReader(request.Body);
    var username = await reader.ReadToEndAsync();
    
    var user = await authService.AuthenticateAsync(username);
    if (user == null)
    {
        user = await authService.RegisterAsync(username);
    }
    return Results.Ok(new { userId = user.Id, username = user.Username });
});

// Map REST API endpoint to retrieve the last 50 messages
app.MapGet("/messages", async (ChatDbContext db) =>
{
    var messages = await db.Messages
        .Include(m => m.User)
        .OrderByDescending(m => m.Timestamp)
        .Take(50)
        .OrderBy(m => m.Timestamp)
        .ToListAsync();
    return Results.Ok(messages);
});

// Map SignalR hub endpoint
app.MapHub<ChatHub>("/chathub");

// Map the default route to serve index.html
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});

app.Run();