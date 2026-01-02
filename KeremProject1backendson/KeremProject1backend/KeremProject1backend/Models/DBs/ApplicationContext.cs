using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Models.DBs
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<AnimeList> AnimeLists { get; set; }
        public virtual DbSet<Tier> Tiers { get; set; }
        public virtual DbSet<RankedItem> RankedItems { get; set; }
        public virtual DbSet<AnimeCache> AnimeCaches { get; set; }
        public virtual DbSet<DataLog> DataLogs { get; set; }
        public virtual DbSet<UserFollow> UserFollows { get; set; }
        public virtual DbSet<ListComment> ListComments { get; set; }
        public virtual DbSet<ListLike> ListLikes { get; set; }
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
                entity.HasMany(u => u.AnimeLists)
                      .WithOne(l => l.AppUser)
                      .HasForeignKey(l => l.AppUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AnimeList>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_AnimeList");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.ShareToken).IsUnique();
                entity.HasMany(l => l.Tiers)
                      .WithOne(t => t.AnimeList)
                      .HasForeignKey(t => t.AnimeListId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tier>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Tier");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(t => t.Items)
                      .WithOne(i => i.Tier)
                      .HasForeignKey(i => i.TierId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RankedItem>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_RankedItem");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.AnimeMalId); // Arama hızı için
            });

            modelBuilder.Entity<AnimeCache>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_AnimeCache");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.MalId).IsUnique(); // Her MAL ID için tek kayıt
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(1000);
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

            modelBuilder.Entity<ListComment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_ListComment");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.AnimeList)
                      .WithMany(l => l.Comments)
                      .HasForeignKey(e => e.AnimeListId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.AppUser)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ListLike>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_ListLike");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.AnimeListId, e.AppUserId }).IsUnique();
                entity.HasOne(e => e.AnimeList)
                      .WithMany(l => l.Likes)
                      .HasForeignKey(e => e.AnimeListId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.AppUser)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(e => e.AppUserId)
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
