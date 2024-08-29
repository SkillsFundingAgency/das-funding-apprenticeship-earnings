using Microsoft.EntityFrameworkCore;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess
{
    public class ApprenticeshipEarningsDataContext : DbContext
    {
        public ApprenticeshipEarningsDataContext(DbContextOptions<ApprenticeshipEarningsDataContext> options) : base(options)
        {
        }

        public virtual DbSet<Earning> Earning { get; set; } = null!;

        public virtual DbSet<ApprenticeshipModel> Apprenticeships { get; set; }
        //public virtual DbSet<EpisodeModel> Episodes { get; set; }
        //public virtual DbSet<EpisodePriceModel> EpisodePrices { get; set; }
        //public virtual DbSet<EarningsProfileModel> EarningsProfiles { get; set; }
        //public virtual DbSet<InstalmentModel> Instalments { get; set; }

        //public virtual DbSet<HistoryRecord<EarningsProfileModelBase>> EarningsProfileHistories { get; set; }
        //public virtual DbSet<InstalmentHistoryModel> InstalmentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EarningsProfileHistories
            modelBuilder.Entity<HistoryRecord<EarningsProfileModelBase>>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<HistoryRecord<EarningsProfileModelBase>>()
                .OwnsOne(x => x.Record)
                .Ignore(x => x.Instalments)
                .ToTable("EarningsProfileHistory");

            // Apprenticeship
            modelBuilder.Entity<ApprenticeshipModel>()
                .HasMany(x => x.Episodes)
                .WithOne()
                .HasForeignKey(fk => fk.ApprenticeshipKey);
            modelBuilder.Entity<ApprenticeshipModel>()
                .HasKey(a => new { a.Key });

            // Episode
            modelBuilder.Entity<EpisodeModel>()
                .HasKey(a => new { a.Key });
            modelBuilder.Entity<EpisodeModel>()
                .Property(p => p.FundingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (FundingType)Enum.Parse(typeof(FundingType), v));

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

            // Instalment
            modelBuilder.Entity<InstalmentModel>()
                .HasKey(x => x.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}
