using Microsoft.EntityFrameworkCore;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess
{
    public class ApprenticeshipEarningsDataContext : DbContext
    {
        public ApprenticeshipEarningsDataContext(DbContextOptions<ApprenticeshipEarningsDataContext> options) : base(options)
        {
        }

        public virtual DbSet<LearningModel> Learnings { get; set; }
        public virtual DbSet<EpisodeModel> Episodes { get; set; }
        public virtual DbSet<EpisodePriceModel> EpisodePrices { get; set; }
        public virtual DbSet<EarningsProfileModel> EarningsProfiles { get; set; }
        public virtual DbSet<InstalmentModel> Instalments { get; set; }
        public virtual DbSet<AdditionalPaymentModel> AdditionalPayments { get; set; }
        public virtual DbSet<EpisodePeriodInLearningModel> EpisodePeriodsInLearnings { get; set; }
        public virtual DbSet<EarningsProfileHistory> EarningsProfileHistories2 { get; set; }
        public virtual DbSet<MathsAndEnglishPeriodInLearningModel> MathsAndEnglishPeriodsInLearning { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apprenticeship
            modelBuilder.Entity<LearningModel>()
                .HasMany(x => x.Episodes)
                .WithOne()
                .HasForeignKey(fk => fk.LearningKey);
            modelBuilder.Entity<LearningModel>()
                .HasKey(a => new { Key = a.LearningKey });

            // Episode
            modelBuilder.Entity<EpisodeModel>()
                .HasKey(a => new { a.Key });
            modelBuilder.Entity<EpisodeModel>()
                .HasOne(a => a.EarningsProfile).WithOne().HasForeignKey<EarningsProfileModel>(x => x.EpisodeKey);
            modelBuilder.Entity<EpisodeModel>()
                .Property(p => p.FundingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (FundingType)Enum.Parse(typeof(FundingType), v));
            modelBuilder.Entity<EpisodeModel>()
                .HasMany(a => a.Prices)
                .WithOne()  
                .HasForeignKey(x => x.EpisodeKey);
            modelBuilder.Entity<EpisodeModel>()
                .HasMany(a => a.PeriodsInLearning)
                .WithOne()
                .HasForeignKey(x => x.EpisodeKey);
            modelBuilder.Entity<EpisodeModel>()
                .Property(p => p.TrainingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (TrainingType)Enum.Parse(typeof(TrainingType), v));

            // EpisodePrice
            modelBuilder.Entity<EpisodePriceModel>()
                .HasKey(x => x.Key);

            // EarningsProfile
            modelBuilder.Entity<EarningsProfileModel>()
                .HasKey(x => x.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileModel>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileModel>()
                .HasMany(x => x.AdditionalPayments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileModel>()
                .HasMany(x => x.MathsAndEnglishCourses)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            // Instalment
            modelBuilder.Entity<InstalmentModel>()
                .HasKey(x => x.Key);

            // EarningsProfileHistory
            modelBuilder.Entity<EarningsProfileHistory>()
                .HasKey(x => x.Key);

            // AdditionalPayment
            modelBuilder.Entity<AdditionalPaymentModel>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<AdditionalPaymentHistoryModel>()
                .HasKey(x => x.Key);

            // MathsAndEnglish
            modelBuilder.Entity<MathsAndEnglishModel>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<MathsAndEnglishModel>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.MathsAndEnglishKey);

            modelBuilder.Entity<MathsAndEnglishModel>()
                .HasMany(x => x.PeriodsInLearning)
                .WithOne()
                .HasForeignKey(fk => fk.MathsAndEnglishKey);

            // MathsAndEnglishInstalment
            modelBuilder.Entity<MathsAndEnglishInstalmentModel>()
                .HasKey(x => x.Key);

            // MathsAndEnglishPeriodInLearningModel
            modelBuilder.Entity<MathsAndEnglishPeriodInLearningModel>()
                .HasKey(x => x.Key);

            // EpisodeBreakInLearning
            modelBuilder.Entity<EpisodePeriodInLearningModel>()
                .HasKey(x => x.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}
