using DateBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DateBoard.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<ProfileView> ProfileViews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OnlineStatus> OnlineStatuses { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Favorite>()
                .HasOne(f => f.Profile)
                .WithMany()
                .HasForeignKey(f => f.ProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PrivateMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PrivateMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.FromUser)
                .WithMany()
                .HasForeignKey(l => l.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.ToProfile)
                .WithMany()
                .HasForeignKey(l => l.ToProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Настройка связей для ProfileView
            builder.Entity<ProfileView>()
                .HasOne(v => v.Viewer)
                .WithMany()
                .HasForeignKey(v => v.ViewerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProfileView>()
                .HasOne(v => v.Profile)
                .WithMany()
                .HasForeignKey(v => v.ProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.FromProfile)
                .WithMany()
                .HasForeignKey(n => n.FromUserId)
                .HasPrincipalKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}