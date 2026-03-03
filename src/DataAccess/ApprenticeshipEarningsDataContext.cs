using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess
{
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
        public virtual DbSet<EarningsProfileHistoryEntity> EarningsProfileHistories2 { get; set; }
        public virtual DbSet<EnglishAndMathsPeriodInLearningEntity> MathsAndEnglishPeriodsInLearning { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apprenticeship
            modelBuilder.Entity<LearningEntity>()
                .HasMany(x => x.ApprenticeshipEpisodes)
                .WithOne()
                .HasForeignKey(fk => fk.LearningKey);
            modelBuilder.Entity<LearningEntity>()
                .HasKey(a => new { Key = a.LearningKey });

            // Episode
            modelBuilder.Entity<ApprenticeshipEpisodeEntity>()
                .HasKey(a => new { a.Key });
            modelBuilder.Entity<ApprenticeshipEpisodeEntity>()
                .HasOne(a => a.EarningsProfile).WithOne().HasForeignKey<ApprenticeshipEarningsProfileEntity>(x => x.EpisodeKey);
            modelBuilder.Entity<ApprenticeshipEpisodeEntity>()
                .Property(p => p.FundingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (FundingType)Enum.Parse(typeof(FundingType), v));
            modelBuilder.Entity<ApprenticeshipEpisodeEntity>()
                .HasMany(a => a.Prices)
                .WithOne()  
                .HasForeignKey(x => x.EpisodeKey);
            modelBuilder.Entity<ApprenticeshipEpisodeEntity>()
                .HasMany(a => a.PeriodsInLearning)
                .WithOne()
                .HasForeignKey(x => x.EpisodeKey);

            // EpisodePrice
            modelBuilder.Entity<ApprenticeshipEpisodePriceEntity>()
                .HasKey(x => x.Key);

            // EarningsProfile
            modelBuilder.Entity<ApprenticeshipEarningsProfileEntity>()
                .HasKey(x => x.EarningsProfileId);

            modelBuilder.Entity<ApprenticeshipEarningsProfileEntity>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            modelBuilder.Entity<ApprenticeshipEarningsProfileEntity>()
                .HasMany(x => x.AdditionalPayments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            modelBuilder.Entity<ApprenticeshipEarningsProfileEntity>()
                .HasMany(x => x.MathsAndEnglishCourses)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            // Instalment
            modelBuilder.Entity<ApprenticeshipInstalmentEntity>()
                .HasKey(x => x.Key);

            // EarningsProfileHistory
            modelBuilder.Entity<EarningsProfileHistoryEntity>()
                .HasKey(x => x.Key);

            // AdditionalPayment
            modelBuilder.Entity<ApprenticeshipAdditionalPaymentEntity>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<AdditionalPaymentHistoryEntity>()
                .HasKey(x => x.Key);

            // MathsAndEnglish
            modelBuilder.Entity<EnglishAndMathsEntity>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<EnglishAndMathsEntity>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.EnglishAndMathsKey);

            modelBuilder.Entity<EnglishAndMathsEntity>()
                .HasMany(x => x.PeriodsInLearning)
                .WithOne()
                .HasForeignKey(fk => fk.EnglishAndMathsKey);

            // MathsAndEnglishInstalment
            modelBuilder.Entity<EnglishAndMathsInstalmentEntity>()
                .HasKey(x => x.Key);

            // MathsAndEnglishPeriodInLearningModel
            modelBuilder.Entity<EnglishAndMathsPeriodInLearningEntity>()
                .HasKey(x => x.Key);

            // EpisodeBreakInLearning
            modelBuilder.Entity<ApprenticeshipPeriodInLearningEntity>()
                .HasKey(x => x.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}
