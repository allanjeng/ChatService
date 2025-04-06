using ChatService.Data;
using ChatService.Hubs;
using ChatService.Services;
using ChatService.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure cache settings
var cacheSettings = builder.Configuration.GetSection("CacheSettings").Get<CacheSettings>();
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

if (cacheSettings == null)
{
    logger.LogWarning("CacheSettings not found in configuration, using default values");
    cacheSettings = new CacheSettings();
}

logger.LogInformation("Configuring cache with settings: SizeLimit={SizeLimit}, ExpirationScanFrequency={ExpirationScanFrequency}, MessageCacheDuration={MessageCacheDuration}",
    cacheSettings.SizeLimit,
    cacheSettings.ExpirationScanFrequency,
    cacheSettings.MessageCacheDuration);

// Add services to the container
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

// Add memory cache with configuration
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = cacheSettings.SizeLimit;
    options.ExpirationScanFrequency = cacheSettings.GetExpirationScanFrequency();
});

// Configure cache settings
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<MessageGeneratorService>();
builder.Services.AddScoped<MessageService>();

// Add controllers
builder.Services.AddControllers();

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

    // Warm up the cache after migrations
    var messageService = scope.ServiceProvider.GetRequiredService<MessageService>();
    await messageService.WarmCacheAsync();
}

// Map controllers
app.MapControllers();

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