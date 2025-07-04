using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using LearningEpisode = SFA.DAS.Learning.Types.LearningEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessEpisodeUpdatedCommandHandler;

[TestFixture]
public class WhenProcessEpisodeUpdatedCommandHandled
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IMessageSession> _mockMessageSession = new();
    private readonly Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<IApprenticeshipRepository> _mockRepository = new();

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
        var apprenticeship = _fixture.BuildApprenticeship();
        SetupMocks();
        var command = BuildCommand(apprenticeship);
        _mockRepository.Setup(x => x.Get(command.EpisodeUpdatedEvent.LearningKey)).ReturnsAsync(apprenticeship);
        var sut = new ProcessUpdatedEpisodeCommand.ProcessEpisodeUpdatedCommandHandler(_mockRepository.Object, _mockMessageSession.Object, _mockEventBuilder.Object, _mockSystemClock.Object);

        // Act
        await sut.Handle(command);

        // Assert
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
        _mockRepository.Verify(x => x.Update(apprenticeship), Times.Once);
    }

    private ProcessEpisodeUpdatedCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.First();
        var priceChangeApprovedEvent = new LearningPriceChangedEvent
        {
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            LearningKey = apprenticeship.ApprenticeshipKey,
            ApprovedBy = ApprovedBy.Employer,
            ApprovedDate = new DateTime(2019, 12, 1),
            EffectiveFromDate = new DateTime(2019, 11, 1),
            Episode = new LearningEpisode
            {
                Key = currentEpisode.ApprenticeshipEpisodeKey,
                Prices = new List<LearningEpisodePrice>()
                {
                    new LearningEpisodePrice
                    {
                        Key = Guid.NewGuid(),
                        EndDate = currentEpisode.Prices.First().EndDate,
                        EndPointAssessmentPrice = 17000,
                        FundingBandMaximum = 21000,
                        StartDate = currentEpisode.Prices.First().StartDate,
                        TrainingPrice = 3000,
                        TotalPrice = 20000
                    }
                },
                EmployerAccountId = currentEpisode.EmployerAccountId,
                Ukprn = 123
            }
        };

        return new ProcessEpisodeUpdatedCommand(priceChangeApprovedEvent);
    }

}