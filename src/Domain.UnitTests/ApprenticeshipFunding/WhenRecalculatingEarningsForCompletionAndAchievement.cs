using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForCompletionAndAchievement
{
    private readonly Fixture _fixture;
    private readonly Mock<ISystemClockService> _mockSystemClock;
    private ApprenticeshipLearning _learning;
    private ApprenticeshipEpisode _episode;
    private decimal _totalPrice;
    private Guid _episodeKey;

    public WhenRecalculatingEarningsForCompletionAndAchievement()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _totalPrice = 15000;
        _learning = _fixture.CreateLearningWithApprenticeship(new DateTime(2021, 8, 1), new DateTime(2022, 7, 31), _totalPrice);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);
        
        _episode = _learning.Episodes.First();
        _episodeKey = _episode.EpisodeKey;
    }

    [Test]
    public void ThenTheCompletionDateDrivesBalancingEarnings()
    {
        // Arrange
        var completionDate = new DateTime(2022, 2, 15); // Period 7
        _episode.UpdateCompletion(completionDate);

        // Act
        _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        // Assert
        var instalments = _episode.EarningsProfile.Instalments;
        
        instalments.Should().ContainSingle(x => x.Type == InstalmentType.Balancing && x.AcademicYear == 2122 && x.DeliveryPeriod == 7 && x.Amount == 6000);
    }

    [Test]
    public void ThenTheAchievementDateDrivesCompletionEarnings()
    {
        // Arrange
        var achievementDate = new DateTime(2022, 3, 15); // Period 8
        _episode.UpdateAchievementDate(achievementDate);

        // Act
        _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        // Assert
        var instalments = _episode.EarningsProfile.Instalments;
        var completionInstalment = instalments.Single(x => x.Type == InstalmentType.Completion);
        completionInstalment.AcademicYear.Should().Be(2122);
        completionInstalment.DeliveryPeriod.Should().Be(8);
        completionInstalment.Amount.Should().Be(3000);
    }

    [Test]
    public void ThenTheCompletionEarningsArePreservedWhenAchievementDateIsAfterCompletionDate()
    {
        // Arrange
        var completionDate = new DateTime(2022, 2, 15); // Period 7
        var achievementDate = new DateTime(2022, 3, 15); // Period 8
        
        _episode.UpdateCompletion(completionDate);
        _episode.UpdateAchievementDate(achievementDate);

        // Act
        _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        // Assert
        var instalments = _episode.EarningsProfile.Instalments;
        
        // Balancing in Period 7
        instalments.Should().ContainSingle(x => x.Type == InstalmentType.Balancing && x.DeliveryPeriod == 7);
        
        // Completion in Period 8
        instalments.Should().ContainSingle(x => x.Type == InstalmentType.Completion && x.DeliveryPeriod == 8);
    }

    [Test]
    public void ThenCompletionEarningsAreNotGeneratedIfAchievementDateIsNull()
    {
        // Arrange
        _episode.UpdateAchievementDate(null);

        // Act
        _learning.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        // Assert
        _episode.EarningsProfile.Instalments.Should().NotContain(x => x.Type == InstalmentType.Completion);
    }
}
