using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Models.DBs
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<DataLog> DataLogs { get; set; }
        public virtual DbSet<UserFollow> UserFollows { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("App");

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_AppUser");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.UserName).IsUnique();
            });

            modelBuilder.Entity<DataLog>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_DataLog");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UserFollow>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_UserFollow");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();
                entity.HasOne(e => e.Follower)
                      .WithMany(u => u.Following)
                      .HasForeignKey(e => e.FollowerId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Following)
                      .WithMany(u => u.Followers)
                      .HasForeignKey(e => e.FollowingId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Notification");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.AppUser)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
