using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenAchievementDateIsSetWithNoInstalments
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private ApprenticeshipLearning _learning;
    private ApprenticeshipEpisode _episode;
    private Guid _episodeKey;

    [SetUp]
    public void SetUp()
    {
        var startDate = new DateTime(2024, 11, 14);
        var completionDate = new DateTime(2024, 11, 25);
        var achievementDate = new DateTime(2025, 1, 28);

        _learning = _fixture.CreateLearningWithApprenticeship(startDate, startDate.AddMonths(2), 0);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        _episode = _learning.Episodes.First();
        _episodeKey = _episode.EpisodeKey;

        _episode.UpdateCompletion(completionDate);
        _episode.UpdateAchievementDate(achievementDate);
    }

    [Test]
    public void ThenItDoesNotThrow()
    {
        var act = () => _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        act.Should().NotThrow();
    }

    [Test]
    public void ThenNoInstalmentsAreGenerated()
    {
        _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        _episode.EarningsProfile.Instalments.Should().BeEmpty();
    }
}
