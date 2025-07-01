using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApprenticeshipEarningsRecalculatedEventBuilder;

[TestFixture]
public class WhenBuilding
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private Command.ApprenticeshipEarningsRecalculatedEventBuilder _builder;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _builder = new Command.ApprenticeshipEarningsRecalculatedEventBuilder(_mockSystemClockService.Object);
    }

    [Test]
    public void ThenTheApprenticeshipEarningsRecalculatedEventIsCorrectlyBuilt()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = _builder.Build(apprenticeship);

        // Assert
        var currentEpisode = apprenticeship.GetCurrentEpisode(_mockSystemClockService.Object);

        result.LearningKey.Should().Be(apprenticeship.LearningKey);
        result.DeliveryPeriods.Should().BeEquivalentTo(currentEpisode.BuildDeliveryPeriods());
        result.EarningsProfileId.Should().Be(currentEpisode.EarningsProfile!.EarningsProfileId);
        result.StartDate.Should().Be(currentEpisode.Prices!.OrderBy(x => x.StartDate).First().StartDate);
        result.PlannedEndDate.Should().Be(currentEpisode.Prices!.OrderBy(x => x.StartDate).Last().EndDate);
        result.AgeAtStartOfLearning.Should().Be(currentEpisode.AgeAtStartOfLearning);
    }
}