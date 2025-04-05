using ChatService.Data;
using ChatService.Hubs;
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

var app = builder.Build();
app.UseStaticFiles();
// Apply any pending migrations and create the database (for demo purposes)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map REST API endpoint to retrieve the last 50 messages
app.MapGet("/messages", async (ChatDbContext db) =>
{
    var messages = await db.Messages
        .OrderByDescending(m => m.Timestamp)
        .Take(50)
        .OrderBy(m => m.Timestamp)
        .ToListAsync();
    return Results.Ok(messages);
});

// Map SignalR hub endpoint
app.MapHub<ChatHub>("/chathub");

app.Run();