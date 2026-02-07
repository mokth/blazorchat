using blazorchat.Models;
using Microsoft.EntityFrameworkCore;

namespace blazorchat.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>()
            .HasKey(message => message.Id);

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(message => new { message.SenderId, message.RecipientId, message.Timestamp });

        modelBuilder.Entity<ChatMessage>()
            .HasIndex(message => new { message.IsGroup, message.Timestamp });

        modelBuilder.Entity<User>()
            .HasKey(user => user.Id);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(user => user.LastSeen);
    }
}
