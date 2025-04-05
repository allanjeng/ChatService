using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }

    // Optionally configure the model further
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // For example, ensure a required relationship between Message and User
        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany() // or .WithMany(u => u.Messages) if you add a collection to User
            .HasForeignKey(m => m.UserId);
    }
    
}