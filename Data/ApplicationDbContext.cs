using DictionaryProvider.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DictionaryProvider.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<DictionaryWord> DictionaryWords => Set<DictionaryWord>();
    public DbSet<VocabularyList> VocabularyLists => Set<VocabularyList>();
    public DbSet<VocabularyListWord> VocabularyListWords => Set<VocabularyListWord>();
    public DbSet<VocabularyListShare> VocabularyListShares => Set<VocabularyListShare>();
    public DbSet<VocabularyListTestResult> VocabularyListTestResults => Set<VocabularyListTestResult>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<LearningCollectionProfile> LearningCollectionProfiles => Set<LearningCollectionProfile>();
    public DbSet<LearningCollectionEntity> LearningCollections => Set<LearningCollectionEntity>();
    public DbSet<LearningCollectionWordEntity> LearningCollectionWords => Set<LearningCollectionWordEntity>();
    public DbSet<FeedbackReport> FeedbackReports => Set<FeedbackReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.NormalizedEmail).HasMaxLength(320).IsRequired();
            entity.Property(user => user.UserName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.NormalizedUserName).HasMaxLength(80).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.HasIndex(user => user.NormalizedEmail).IsUnique();
            entity.HasIndex(user => user.NormalizedUserName);
        });

        modelBuilder.Entity<DictionaryWord>(entity =>
        {
            entity.ToTable("dictionary_words");
            entity.HasKey(word => word.Id);
            entity.Property(word => word.Word).HasMaxLength(160).IsRequired();
            entity.Property(word => word.NormalizedWord).HasMaxLength(160).IsRequired();
            entity.Property(word => word.Provider).HasMaxLength(80);
            entity.Property(word => word.CreatedAt).IsRequired();
            entity.HasIndex(word => word.NormalizedWord).IsUnique();
        });

        modelBuilder.Entity<VocabularyList>(entity =>
        {
            entity.ToTable("vocabulary_lists");
            entity.HasKey(list => list.Id);
            entity.Property(list => list.Name).HasMaxLength(120).IsRequired();
            entity.Property(list => list.Description).HasMaxLength(600);
            entity.Property(list => list.CreatedAt).IsRequired();
            entity.Property(list => list.UpdatedAt).IsRequired();
            entity.HasIndex(list => new { list.OwnerId, list.Name });
            entity.HasOne(list => list.Owner)
                .WithMany()
                .HasForeignKey(list => list.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyListWord>(entity =>
        {
            entity.ToTable("vocabulary_list_words");
            entity.HasKey(word => word.Id);
            entity.Property(word => word.DisplayWord).HasMaxLength(160).IsRequired();
            entity.Property(word => word.NormalizedWord).HasMaxLength(160).IsRequired();
            entity.Property(word => word.AddedAt).IsRequired();
            entity.HasIndex(word => new { word.VocabularyListId, word.NormalizedWord }).IsUnique();
            entity.HasOne(word => word.VocabularyList)
                .WithMany(list => list.Words)
                .HasForeignKey(word => word.VocabularyListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(word => word.DictionaryWord)
                .WithMany()
                .HasForeignKey(word => word.DictionaryWordId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VocabularyListShare>(entity =>
        {
            entity.ToTable("vocabulary_list_shares");
            entity.HasKey(share => share.Id);
            entity.Property(share => share.Permission)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(share => share.SharedAt).IsRequired();
            entity.HasIndex(share => new { share.VocabularyListId, share.UserId }).IsUnique();
            entity.HasOne(share => share.VocabularyList)
                .WithMany(list => list.Shares)
                .HasForeignKey(share => share.VocabularyListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(share => share.User)
                .WithMany()
                .HasForeignKey(share => share.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularyListTestResult>(entity =>
        {
            entity.ToTable("vocabulary_list_test_results");
            entity.HasKey(result => result.Id);
            entity.Property(result => result.CompletedAt).IsRequired();
            entity.HasIndex(result => new { result.UserId, result.VocabularyListId, result.CompletedAt });
            entity.HasOne(result => result.VocabularyList)
                .WithMany(list => list.TestResults)
                .HasForeignKey(result => result.VocabularyListId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(result => result.User)
                .WithMany()
                .HasForeignKey(result => result.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppNotification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Type).HasMaxLength(80).IsRequired();
            entity.Property(notification => notification.Title).HasMaxLength(160).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(600).IsRequired();
            entity.Property(notification => notification.CreatedAt).IsRequired();
            entity.HasIndex(notification => new { notification.RecipientUserId, notification.IsRead, notification.CreatedAt });
            entity.HasOne(notification => notification.RecipientUser)
                .WithMany()
                .HasForeignKey(notification => notification.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(notification => notification.RelatedList)
                .WithMany()
                .HasForeignKey(notification => notification.RelatedListId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningCollectionProfile>(entity =>
        {
            entity.ToTable("learning_collection_profiles");
            entity.HasKey(profile => profile.UserId);
            entity.Property(profile => profile.UpdatedAt).IsRequired();
            entity.HasOne(profile => profile.User).WithOne()
                .HasForeignKey<LearningCollectionProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningCollectionEntity>(entity =>
        {
            entity.ToTable("learning_collections");
            entity.HasKey(collection => collection.Id);
            entity.Property(collection => collection.Name).HasMaxLength(60).IsRequired();
            entity.Property(collection => collection.Accent).HasMaxLength(20).IsRequired();
            entity.HasIndex(collection => new { collection.ProfileUserId, collection.Name }).IsUnique();
            entity.HasOne(collection => collection.Profile).WithMany(profile => profile.Collections)
                .HasForeignKey(collection => collection.ProfileUserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LearningCollectionWordEntity>(entity =>
        {
            entity.ToTable("learning_collection_words");
            entity.HasKey(word => word.Id);
            entity.Property(word => word.Word).HasMaxLength(160).IsRequired();
            entity.Property(word => word.NormalizedWord).HasMaxLength(160).IsRequired();
            entity.Property(word => word.State).IsRequired();
            entity.Property(word => word.Level).HasMaxLength(32);
            entity.Property(word => word.Definition).HasMaxLength(2000);
            entity.Property(word => word.Translation).HasMaxLength(1000);
            entity.Property(word => word.AddedAt).IsRequired();
            entity.HasIndex(word => new { word.CollectionId, word.NormalizedWord }).IsUnique();
            entity.HasOne(word => word.Collection).WithMany(collection => collection.Words)
                .HasForeignKey(word => word.CollectionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeedbackReport>(entity =>
        {
            entity.ToTable("feedback_reports");
            entity.HasKey(report => report.Id);
            entity.Property(report => report.Type).HasMaxLength(80).IsRequired();
            entity.Property(report => report.Description).HasMaxLength(4000).IsRequired();
            entity.Property(report => report.PageOrFeature).HasMaxLength(240);
            entity.Property(report => report.ContactEmail).HasMaxLength(320);
            entity.Property(report => report.Diagnostics).HasMaxLength(2000);
            entity.Property(report => report.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(report => report.CreatedAt).IsRequired();
            entity.Property(report => report.UpdatedAt);
            entity.HasIndex(report => report.CreatedAt);
            entity.HasIndex(report => report.Status);
            entity.HasOne(report => report.User)
                .WithMany()
                .HasForeignKey(report => report.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
