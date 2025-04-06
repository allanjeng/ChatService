using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Data;

/// <summary>
/// Database context for the chat application.
/// This context manages the database interactions for users and messages,
/// including entity relationships and database schema configuration.
/// </summary>
/// <param name="options">The options to be used by the DbContext</param>
public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Set of users in the system.
    /// Provides access to user-related database operations.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Set of messages in the system.
    /// Provides access to message-related database operations.
    /// </summary>
    public DbSet<Message> Messages { get; set; } = null!;

    /// <summary>
    /// Configures the model relationships and constraints.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}