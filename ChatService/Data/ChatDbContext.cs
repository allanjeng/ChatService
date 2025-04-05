using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Data;

/// <summary>
/// Database context for the chat application.
/// </summary>
public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{

    /// <summary>
    /// Set of users in the system.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Set of messages in the system.
    /// </summary>
    public DbSet<Message> Messages { get; set; } = null!;

    // Optionally configure the model further
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // For example, ensure a required relationship between Message and User
        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany() // or .WithMany(u => u.Messages) if you add a collection to User
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}