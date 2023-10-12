using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealTimeMinimalChatAPI.Models.Domain;

namespace RealTimeMinimalChatAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReceiver> MessageReceivers { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MessageReceiver>()
                .HasKey(mr => new { mr.MessageId, mr.ReceiverId });

            modelBuilder.Entity<MessageReceiver>()
                .HasOne(mr => mr.Message)
                .WithMany(m => m.Receivers)
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MessageReceiver>()
                .HasOne(mr => mr.Receiver)
                .WithMany(m => m.Receivers)
                .HasForeignKey(mr => mr.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<ChatUser>()
            .HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId)
              .OnDelete(DeleteBehavior.Restrict);
            // Remove cascade delete
            // or .OnDelete(DeleteBehavior.Restrict) based on your requirements

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ChatUsers)
                .HasForeignKey(cu => cu.UserId);
                

        }
        
    }
    

}
