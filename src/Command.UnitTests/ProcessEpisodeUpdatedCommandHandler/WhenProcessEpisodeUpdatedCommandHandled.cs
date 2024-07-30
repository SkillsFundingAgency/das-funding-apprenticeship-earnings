using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using ApprenticeshipEpisode = SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessEpisodeUpdatedCommandHandler;

[TestFixture]
public class WhenProcessEpisodeUpdatedCommandHandled
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IMessageSession> _mockMessageSession = new();
    private readonly Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();

    private void SetupMocks()
    {
        _mockMessageSession.Reset();
        _mockEventBuilder.Reset();
        _mockEventBuilder.Setup(x => x.Build(It.IsAny<Apprenticeship>())).Returns(new ApprenticeshipEarningsRecalculatedEvent());
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2019, 12, 1));
    }

    [Test]
    public async Task ThenTheEarningsAreGenerated()
    {
        // Arrange
        SetupMocks();
        var command = BuildCommand();
        var sut = new ProcessUpdatedEpisodeCommand.ProcessEpisodeUpdatedCommandHandler(_mockMessageSession.Object, _mockEventBuilder.Object, _mockSystemClock.Object);

        // Act
        var apprenticeship = await sut.RecalculateEarnings(command);

        // Assert
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private ProcessEpisodeUpdatedCommand BuildCommand()
    {
        var apprenticeship = _fixture.CreateApprenticeshipEntityModel();
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.Single();
        var priceChangeApprovedEvent = new ApprenticeshipPriceChangedEvent
        {
            ApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ApprovedBy = ApprovedBy.Employer,
            ApprovedDate = new DateTime(2019, 12, 1),
            EffectiveFromDate = new DateTime(2019, 11, 1),
            Episode = new ApprenticeshipEpisode
            {
                Key = currentEpisode.ApprenticeshipEpisodeKey,
                Prices = new List<ApprenticeshipEpisodePrice>()
                {
                    new ApprenticeshipEpisodePrice
                    {
                        Key = Guid.NewGuid(),
                        EndDate = currentEpisode.Prices!.Single().PlannedEndDate,
                        EndPointAssessmentPrice = 17000,
                        FundingBandMaximum = 21000,
                        StartDate = currentEpisode.Prices.Single().ActualStartDate,
                        TrainingPrice = 3000,
                        TotalPrice = 20000
                    }
                },
                EmployerAccountId = currentEpisode.EmployerAccountId,
                Ukprn = 123
            }
        };

        return new ProcessEpisodeUpdatedCommand(apprenticeship, priceChangeApprovedEvent);
    }
}