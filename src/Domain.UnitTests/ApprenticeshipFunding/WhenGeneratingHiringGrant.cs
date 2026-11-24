using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenGeneratingHiringGrant
{
    private Mock<ISystemClockService> _systemClock = null!;

    [SetUp]
    public void SetUp()
    {
        _systemClock = new Mock<ISystemClockService>();
        _systemClock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 11, 1));
    }

    [Test]
    public void Then_The_Grant_Rows_Are_Added_For_A_Non_Levy_Eligible_Episode()
    {
        var (learning, episode) = CreateLearning(EmployerType.NonLevy);

        learning.GenerateHiringGrant(episode.EpisodeKey, _systemClock.Object);

        GrantsOf(episode).Select(x => x.DueDate).Should().Equal(new DateTime(2026, 12, 29), new DateTime(2027, 9, 30));
    }

    [Test]
    public void Then_No_Grant_Rows_Are_Added_For_A_Levy_Episode()
    {
        var (learning, episode) = CreateLearning(EmployerType.Levy);

        learning.GenerateHiringGrant(episode.EpisodeKey, _systemClock.Object);

        GrantsOf(episode).Should().BeEmpty();
    }

    [Test]
    public void Then_Generating_Twice_Does_Not_Duplicate_The_Grant()
    {
        var (learning, episode) = CreateLearning(EmployerType.NonLevy);

        learning.GenerateHiringGrant(episode.EpisodeKey, _systemClock.Object);
        learning.GenerateHiringGrant(episode.EpisodeKey, _systemClock.Object);

        GrantsOf(episode).Should().HaveCount(2);
    }

    [Test]
    public void Then_The_Grant_Survives_A_Recalculation_Of_The_Earnings()
    {
        var (learning, episode) = CreateLearning(EmployerType.NonLevy);
        learning.Calculate(_systemClock.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: false);
        learning.GenerateHiringGrant(episode.EpisodeKey, _systemClock.Object);

        learning.Calculate(_systemClock.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: false);

        GrantsOf(episode).Should().HaveCount(2);
    }

    private static System.Collections.Generic.List<Domain.Models.Apprenticeship.AdditionalPayment> GrantsOf(ApprenticeshipEpisode episode) =>
        episode.EarningsProfile!.AdditionalPayments.Where(x => x.AdditionalPaymentType == InstalmentTypes.EmployerHiringGrant).OrderBy(x => x.DueDate).ToList();

    private static (ApprenticeshipLearning, ApprenticeshipEpisode) CreateLearning(EmployerType employerType)
    {
        var episodeKey = Guid.NewGuid();
        var entity = new ApprenticeshipLearningEntity
        {
            LearningKey = Guid.NewGuid(),
            DateOfBirth = new DateTime(2008, 1, 1),
            Episodes =
            [
                new ApprenticeshipEpisodeEntity
                {
                    Key = episodeKey,
                    EmployerType = employerType,
                    FundingBandMaximum = 15000,
                    Prices = [new ApprenticeshipEpisodePriceEntity { Key = Guid.NewGuid(), EpisodeKey = episodeKey, StartDate = new DateTime(2026, 10, 1), EndDate = new DateTime(2028, 9, 30), AgreedPrice = 12000 }],
                    PeriodsInLearning = [new ApprenticeshipPeriodInLearningEntity { Key = Guid.NewGuid(), EpisodeKey = episodeKey, StartDate = new DateTime(2026, 10, 1), OriginalExpectedEndDate = new DateTime(2028, 9, 30) }],
                    EarningsProfile = new ApprenticeshipEarningsProfileEntity
                    {
                        EarningsProfileId = Guid.NewGuid(),
                        EpisodeKey = episodeKey,
                        CalculationData = "{}",
                        Instalments = [],
                        ApprenticeshipAdditionalPayments = [],
                        EnglishAndMathsCourses = []
                    }
                }
            ]
        };

        var learning = ApprenticeshipLearning.Get(entity);
        return (learning, learning.Episodes.Single());
    }
}
