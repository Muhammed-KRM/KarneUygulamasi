using KeremProject1backend.Models.DBs;
using Microsoft.EntityFrameworkCore;

namespace KeremProject1backend.Infrastructure;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }

    // DbSets for Phase 1
    public DbSet<User> Users { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<InstitutionUser> InstitutionUsers { get; set; }
    public DbSet<AccountLink> AccountLinks { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);

            entity.Property(e => e.GlobalRole).HasConversion<byte>();
            entity.Property(e => e.Status).HasConversion<byte>();
            entity.Property(e => e.ProfileVisibility).HasConversion<byte>();
        });

        // Institution Configuration
        modelBuilder.Entity<Institution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LicenseNumber).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LicenseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.Property(e => e.Status).HasConversion<byte>();

            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InstitutionUser Configuration (Many-to-Many with Role)
        modelBuilder.Entity<InstitutionUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.InstitutionId }).IsUnique();
            entity.HasIndex(e => e.StudentNumber);
            entity.HasIndex(e => e.EmployeeNumber);

            entity.Property(e => e.StudentNumber).HasMaxLength(50);
            entity.Property(e => e.EmployeeNumber).HasMaxLength(50);
            entity.Property(e => e.Role).HasConversion<byte>();

            entity.HasOne(e => e.User)
                .WithMany(u => u.InstitutionMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Institution)
                .WithMany(i => i.Members)
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AccountLink Configuration
        modelBuilder.Entity<AccountLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MainUserId, e.InstitutionUserId }).IsUnique();

            entity.Property(e => e.Status).HasConversion<byte>();

            entity.HasOne(e => e.MainUser)
                .WithMany(u => u.AccountLinks)
                .HasForeignKey(e => e.MainUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.InstitutionUser)
                .WithMany()
                .HasForeignKey(e => e.InstitutionUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });
    }
}
