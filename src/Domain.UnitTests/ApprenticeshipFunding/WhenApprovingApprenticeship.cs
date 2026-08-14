using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

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
}
