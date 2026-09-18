using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenApprovingApprenticeship
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
    }

    [Test]
    public void ThenTheEpisodeEarningsProfileIsApproved()
    {
        var apprenticeship = _fixture.CreateLearning();
        apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        var episode = apprenticeship.Episodes.Single();

        apprenticeship.Approve(
            episode.EpisodeKey,
            _fixture.Create<long>(),
            _fixture.Create<long>(),
            _fixture.Create<Guid>(),
            _fixture.Create<string>());

        episode.EarningsProfile!.IsApproved.Should().BeTrue();
    }

    [Test]
    public void ThenTheEmployerAccountIdAndFundingEmployerAccountIdAreSet()
    {
        var apprenticeship = _fixture.CreateLearning();
        apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        var episode = apprenticeship.Episodes.Single();

        var employerAccountId = _fixture.Create<long>();
        var fundingAccountId = _fixture.Create<long>();

        apprenticeship.Approve(
            episode.EpisodeKey,
            employerAccountId,
            fundingAccountId,
            _fixture.Create<Guid>(),
            _fixture.Create<string>());

        episode.EmployerAccountId.Should().Be(employerAccountId);
        episode.FundingEmployerAccountId.Should().Be(fundingAccountId);
    }

    [Test]
    public void ThenApprenticeshipPayableEarningsUpdatedEventIsQueuedWhenLearnerRefIsPresent()
    {
        var apprenticeship = _fixture.CreateLearning();
        apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        var episode = apprenticeship.Episodes.Single();
        apprenticeship.FlushEvents();

        var employerAccountId = _fixture.Create<long>();
        var fundingAccountId = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();
        var learnerRef = _fixture.Create<string>();

        apprenticeship.Approve(
            episode.EpisodeKey,
            employerAccountId,
            fundingAccountId,
            learnerKey,
            learnerRef);

        var @event = apprenticeship.FlushEvents().OfType<ApprenticeshipPayableEarningsUpdatedEvent>().Single();

        @event.LearningKey.Should().Be(apprenticeship.LearningKey);
        @event.EpisodeKey.Should().Be(episode.EpisodeKey);
        @event.EmployerAccountId.Should().Be(employerAccountId);
        @event.FundingAccountId.Should().Be(fundingAccountId);
        @event.LearnerKey.Should().Be(learnerKey);
        @event.LearnerRef.Should().Be(learnerRef);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ThenApprenticeshipPayableEarningsUpdatedEventIsNotQueuedWhenLearnerRefIsAbsent(string? learnerRef)
    {
        var apprenticeship = _fixture.CreateLearning();
        apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        var episode = apprenticeship.Episodes.Single();
        apprenticeship.FlushEvents();

        apprenticeship.Approve(
            episode.EpisodeKey,
            _fixture.Create<long>(),
            _fixture.Create<long>(),
            _fixture.Create<Guid>(),
            learnerRef!);

        var events = apprenticeship.FlushEvents();
        events.OfType<ApprenticeshipPayableEarningsUpdatedEvent>().Should().BeEmpty();
    }
}
