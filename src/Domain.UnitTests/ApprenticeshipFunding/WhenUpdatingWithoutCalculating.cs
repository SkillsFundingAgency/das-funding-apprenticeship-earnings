using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using ApprenticeshipEarningsRecalculatedEvent = SFA.DAS.Funding.ApprenticeshipEarnings.Types.ApprenticeshipEarningsRecalculatedEvent;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenUpdatingWithoutCalculating
{
    private Fixture _fixture = null!;
    private Mock<ISystemClockService> _systemClock = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _systemClock = new Mock<ISystemClockService>();
        _systemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
    }

    [Test]
    public void Then_A_Blank_Profile_Is_Created_When_There_Is_No_Profile()
    {
        var (learning, episode) = CreateLearning();

        episode.UpdateWithoutCalculating(learning, _systemClock.Object);

        episode.EarningsProfile.Should().NotBeNull();
        episode.EarningsProfile!.Instalments.Should().BeEmpty();
        episode.UsingEarnAndPayCalc.Should().BeFalse();
    }

    [Test]
    public void Then_Calculated_Earnings_Are_Cleared_And_The_Flag_Is_Switched_Off()
    {
        var (learning, episode) = CreateLearning();
        learning.Calculate(_systemClock.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: false);
        var profileKey = episode.EarningsProfile!.EarningsProfileId;
        episode.EarningsProfile.Instalments.Should().NotBeEmpty();

        episode.UpdateWithoutCalculating(learning, _systemClock.Object);

        episode.UsingEarnAndPayCalc.Should().BeFalse();
        episode.IsRemoved.Should().BeFalse();
        episode.EarningsProfile!.EarningsProfileId.Should().Be(profileKey);
        episode.EarningsProfile.Instalments.Should().BeEmpty();
        episode.EarningsProfile.AdditionalPayments.Should().BeEmpty();
        episode.EarningsProfile.OnProgramTotal.Should().Be(0);
        episode.EarningsProfile.CompletionPayment.Should().Be(0);
    }

    [Test]
    public void Then_A_Recalculated_Event_Is_Raised_When_Earnings_Were_Cleared()
    {
        var (learning, episode) = CreateLearning();
        learning.Calculate(_systemClock.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: false);
        learning.FlushEvents();

        episode.UpdateWithoutCalculating(learning, _systemClock.Object);

        learning.FlushEvents().OfType<ApprenticeshipEarningsRecalculatedEvent>().Should().ContainSingle();
    }

    [Test]
    public void Then_No_Recalculated_Event_Is_Raised_When_There_Were_No_Earnings()
    {
        var (learning, episode) = CreateLearning();
        episode.CreateBlankEarningsProfile(isApproved: false);
        learning.FlushEvents();

        episode.UpdateWithoutCalculating(learning, _systemClock.Object);

        learning.FlushEvents().OfType<ApprenticeshipEarningsRecalculatedEvent>().Should().BeEmpty();
        episode.UsingEarnAndPayCalc.Should().BeFalse();
    }

    [Test]
    public void Then_An_Approved_Episode_Cannot_Have_Its_Earnings_Cleared()
    {
        var (learning, episode) = CreateLearning();
        learning.Calculate(_systemClock.Object, "{}", episode.EpisodeKey, initialGenerationIsApproved: true);

        var act = () => episode.UpdateWithoutCalculating(learning, _systemClock.Object);

        act.Should().Throw<InvalidOperationException>();
        episode.EarningsProfile!.Instalments.Should().NotBeEmpty();
        episode.UsingEarnAndPayCalc.Should().BeTrue();
    }

    private (ApprenticeshipLearning learning, ApprenticeshipEpisode episode) CreateLearning()
    {
        var learning = _fixture.CreateLearningWithApprenticeship(new DateTime(2021, 1, 15), new DateTime(2022, 1, 31), 15000m);
        return (learning, learning.Episodes.First());
    }
}
