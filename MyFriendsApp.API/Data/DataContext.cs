using MyFriendsApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MyFriendsApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Like>().HasKey(k => new { k.LikerId, k.LikeeId });
            builder.Entity<Like>()
                .HasOne(k => k.Likee)
                .WithMany(k => k.Likers)
                .HasForeignKey(k => k.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
            .HasOne(k => k.Liker)
            .WithMany(k => k.Likees)
            .HasForeignKey(k => k.LikerId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
            .HasOne(k => k.Sender)
            .WithMany(k => k.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Message>()
            .HasOne(k => k.Recipient)
            .WithMany(k => k.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}