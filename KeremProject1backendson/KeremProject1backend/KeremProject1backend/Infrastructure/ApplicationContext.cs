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

    // Authentication & Security
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }

    // DbSets for Phase 2
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomStudent> ClassroomStudents { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamResult> ExamResults { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMember> ConversationMembers { get; set; }
    public DbSet<Message> Messages { get; set; }

    // DbSets for Phase 3 (Social Network & Discovery)
    public DbSet<Content> Contents { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<Mute> Mutes { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<StoryView> StoryViews { get; set; }
    public DbSet<StoryReaction> StoryReactions { get; set; }
    public DbSet<ContentReport> ContentReports { get; set; }
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollVote> PollVotes { get; set; }
    public DbSet<ContentDraft> ContentDrafts { get; set; }

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

        // Phase 2: Academic & Messaging Configurations

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.Institution)
                .WithMany()
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.HeadTeacher)
                .WithMany()
                .HasForeignKey(e => e.HeadTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ClassConversation)
                .WithOne(e => e.Classroom)
                .HasForeignKey<Conversation>(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ClassroomStudent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ClassroomId, e.InstitutionUserId }).IsUnique();
            entity.HasIndex(e => e.RemovedAt); // For soft delete queries
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to avoid multiple paths from Institution
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.InstitutionUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasConversion<byte>();
            entity.HasOne(e => e.Institution)
                .WithMany()
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Exams)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ExamId, e.StudentId }).IsUnique();
            entity.HasIndex(e => e.StudentNumber);
            entity.HasOne(e => e.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(e => e.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Topics)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<byte>();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.HasOne(e => e.Institution)
                .WithMany()
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ConversationId, e.UserId }).IsUnique();
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<byte>();
            entity.Property(e => e.Content).HasMaxLength(5000).IsRequired();
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.SentAt);
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Authentication & Security Configurations
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Token).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Token).HasMaxLength(100).IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserPreferences Configuration
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Theme).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Language).HasMaxLength(10).IsRequired();
            entity.Property(e => e.DateFormat).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TimeFormat).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ProfileLayout).HasMaxLength(2000);
            entity.Property(e => e.DashboardLayout).HasMaxLength(2000);
            entity.Property(e => e.VisibleWidgets).HasMaxLength(1000);
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<UserPreferences>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 3: Social Network & Discovery Configurations

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.LessonId);
            entity.HasIndex(e => e.TopicId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.IsDeleted, e.CreatedAt });

            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.TagsJson).HasMaxLength(2000);

            entity.Property(e => e.ContentType).HasConversion<byte>();
            entity.Property(e => e.Difficulty).HasConversion<byte>();

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Lesson)
                .WithMany()
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Topic)
                .WithMany()
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SolvedByUser)
                .WithMany()
                .HasForeignKey(e => e.SolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.ParentCommentId);
            entity.HasIndex(e => new { e.IsDeleted, e.CreatedAt });

            entity.Property(e => e.Text).HasMaxLength(2000).IsRequired();

            entity.HasOne(e => e.Content)
                .WithMany(c => c.Comments)
                .HasForeignKey(e => e.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ContentId, e.Type }).IsUnique();
            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Type).HasConversion<byte>();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Content)
                .WithMany(c => c.Interactions)
                .HasForeignKey(e => e.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FollowerId, e.FollowingId }).IsUnique();
            entity.HasIndex(e => e.FollowerId);
            entity.HasIndex(e => e.FollowingId);

            entity.HasOne(e => e.Follower)
                .WithMany()
                .HasForeignKey(e => e.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Following)
                .WithMany()
                .HasForeignKey(e => e.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.BlockerId, e.BlockedId }).IsUnique();
            entity.HasIndex(e => e.BlockerId);
            entity.HasIndex(e => e.BlockedId);

            entity.HasOne(e => e.Blocker)
                .WithMany()
                .HasForeignKey(e => e.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Blocked)
                .WithMany()
                .HasForeignKey(e => e.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Mute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.MutedUserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MutedUserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MutedUser)
                .WithMany()
                .HasForeignKey(e => e.MutedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => new { e.IsDeleted, e.ExpiresAt });
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.Text).HasMaxLength(200);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StoryId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.StoryId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Story)
                .WithMany(s => s.Views)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoryReaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StoryId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.StoryId);
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Reaction).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.Story)
                .WithMany(s => s.Reactions)
                .HasForeignKey(e => e.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContentReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ContentId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Reason).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReviewNotes).HasMaxLength(2000);

            entity.Property(e => e.Status).HasConversion<byte>();

            entity.HasOne(e => e.Content)
                .WithMany()
                .HasForeignKey(e => e.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Question).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OptionsJson).IsRequired();

            entity.HasOne(e => e.Content)
                .WithMany()
                .HasForeignKey(e => e.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PollVote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PollId, e.UserId, e.OptionIndex }).IsUnique(); // Aynı kullanıcı aynı seçeneği birden fazla kez seçemez
            entity.HasIndex(e => e.PollId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(e => e.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContentDraft>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.LastSavedAt);

            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(5000);
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            entity.Property(e => e.VideoUrl).HasMaxLength(1000);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);

            entity.Property(e => e.ContentType).HasConversion<byte>();
            entity.Property(e => e.Difficulty).HasConversion<byte>();

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Lesson)
                .WithMany()
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Topic)
                .WithMany()
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
