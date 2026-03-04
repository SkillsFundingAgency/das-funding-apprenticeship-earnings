using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Learning.Types;
using System.Reflection.Emit;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;

public class ApprenticeshipEarningsDataContext : DbContext
{
    public ApprenticeshipEarningsDataContext(DbContextOptions<ApprenticeshipEarningsDataContext> options) : base(options)
    {
    }

    public virtual DbSet<LearningEntity> Learnings { get; set; }
    public virtual DbSet<ApprenticeshipEpisodeEntity> ApprenticeshipEpisodes { get; set; }
    public virtual DbSet<ShortCourseEpisodeEntity> ShortCourseEpisodes { get; set; }
    public virtual DbSet<ApprenticeshipEpisodePriceEntity> EpisodePrices { get; set; }
    public virtual DbSet<ApprenticeshipEarningsProfileEntity> EarningsProfiles { get; set; }
    public virtual DbSet<ApprenticeshipInstalmentEntity> Instalments { get; set; }
    public virtual DbSet<ApprenticeshipAdditionalPaymentEntity> AdditionalPayments { get; set; }
    public virtual DbSet<ApprenticeshipPeriodInLearningEntity> EpisodePeriodsInLearnings { get; set; }
    public virtual DbSet<ApprenticeshipEarningsProfileHistoryEntity> EarningsProfileHistories2 { get; set; }
    public virtual DbSet<ShortCourseEarningsProfileHistoryEntity> ShortCourseEarningsProfileHistories { get; set; }
    public virtual DbSet<EnglishAndMathsPeriodInLearningEntity> MathsAndEnglishPeriodsInLearning { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningEntity>().Configure();

        //  Apprenticeship
        modelBuilder.Entity<ApprenticeshipEpisodeEntity>().Configure();
        modelBuilder.Entity<ApprenticeshipEpisodePriceEntity>().HasKey(x => x.Key);
        modelBuilder.Entity<ApprenticeshipEarningsProfileEntity>().Configure();
        modelBuilder.Entity<ApprenticeshipInstalmentEntity>().HasKey(x => x.Key);
        modelBuilder.Entity<ApprenticeshipAdditionalPaymentEntity>().HasKey(x => x.Key);
        modelBuilder.Entity<ApprenticeshipPeriodInLearningEntity>().HasKey(x => x.Key);

        //  English and Maths
        modelBuilder.Entity<EnglishAndMathsEntity>().Configure();
        modelBuilder.Entity<EnglishAndMathsInstalmentEntity>().HasKey(x => x.Key);
        modelBuilder.Entity<EnglishAndMathsPeriodInLearningEntity>().HasKey(x => x.Key);

        //  Apprenticeship History
        modelBuilder.Entity<ApprenticeshipEarningsProfileHistoryEntity>().HasKey(x => x.Key);
        modelBuilder.Entity<AdditionalPaymentHistoryEntity>().HasKey(x => x.Key);

        //  Short Course
        modelBuilder.Entity<ShortCourseEpisodeEntity>().Configure();
        modelBuilder.Entity<ShortCourseEarningsProfileEntity>().Configure();
        modelBuilder.Entity<ShortCourseInstalmentEntity>().HasKey(x => x.Key);

        //  Short Course History
        modelBuilder.Entity<ShortCourseEarningsProfileHistoryEntity>().HasKey(x => x.Key);

        base.OnModelCreating(modelBuilder);
    }
}

internal static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<LearningEntity> Configure(this EntityTypeBuilder<LearningEntity> builder)
    {
        builder
            .HasMany(x => x.ApprenticeshipEpisodes)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);

        builder
            .HasMany(x => x.ShortCourseEpisodes)
            .WithOne()
            .HasForeignKey(fk => fk.LearningKey);

        builder
            .HasKey(a => new { Key = a.LearningKey });

        return builder;
    }

    public static EntityTypeBuilder<ApprenticeshipEpisodeEntity> Configure(this EntityTypeBuilder<ApprenticeshipEpisodeEntity> builder)
    {
        builder.HasKey(a => new { a.Key });

        builder
            .HasOne(a => a.EarningsProfile)
            .WithOne()
            .HasForeignKey<ApprenticeshipEarningsProfileEntity>(x => x.EpisodeKey);

        builder
            .Property(p => p.FundingType)
            .HasConversion(
                v => v.ToString(),
                v => (FundingType)Enum.Parse(typeof(FundingType), v));

        builder
            .HasMany(a => a.Prices)
            .WithOne()
            .HasForeignKey(x => x.EpisodeKey);

        builder
            .HasMany(a => a.PeriodsInLearning)
            .WithOne()
            .HasForeignKey(x => x.EpisodeKey);

        return builder;
    }

    public static EntityTypeBuilder<ApprenticeshipEarningsProfileEntity> Configure(this EntityTypeBuilder<ApprenticeshipEarningsProfileEntity> builder)
    {
        builder
            .HasKey(x => x.EarningsProfileId);

        builder
            .HasMany(x => x.Instalments)
            .WithOne()
            .HasForeignKey(fk => fk.EarningsProfileId);

        builder
            .HasMany(x => x.ApprenticeshipAdditionalPayments)
            .WithOne()
            .HasForeignKey(fk => fk.EarningsProfileId);

        builder
            .HasMany(x => x.EnglishAndMathsCourses)
            .WithOne()
            .HasForeignKey(fk => fk.EarningsProfileId);

        return builder;
    }

    public static EntityTypeBuilder<EnglishAndMathsEntity> Configure(this EntityTypeBuilder<EnglishAndMathsEntity> builder)
    {
        builder
            .HasKey(x => x.Key);

        builder
            .HasMany(x => x.Instalments)
            .WithOne()
            .HasForeignKey(fk => fk.EnglishAndMathsKey);

        builder
            .HasMany(x => x.PeriodsInLearning)
            .WithOne()
            .HasForeignKey(fk => fk.EnglishAndMathsKey);

        return builder;
    }

    public static EntityTypeBuilder<ShortCourseEpisodeEntity> Configure(this EntityTypeBuilder<ShortCourseEpisodeEntity> builder)
    {
        builder.HasKey(a => new { a.Key });

        builder
            .HasOne(a => a.EarningsProfile)
            .WithOne()
            .HasForeignKey<ShortCourseEarningsProfileEntity>(x => x.EpisodeKey);

        builder
            .Property(p => p.FundingType)
            .HasConversion(
                v => v.ToString(),
                v => (FundingType)Enum.Parse(typeof(FundingType), v));

        return builder;
    }

    public static EntityTypeBuilder<ShortCourseEarningsProfileEntity> Configure(this EntityTypeBuilder<ShortCourseEarningsProfileEntity> builder)
    {
        builder
            .HasKey(x => x.EarningsProfileId);

        builder
            .HasMany(x => x.Instalments)
            .WithOne()
            .HasForeignKey(fk => fk.EarningsProfileId);

        return builder;
    }

}