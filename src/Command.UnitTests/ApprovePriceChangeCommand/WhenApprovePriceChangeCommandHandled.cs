using AutoFixture;
using Microsoft.Extensions.Internal;
using Moq;
using NServiceBus;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApprovePriceChangeCommand;

[TestFixture]
public class WhenApprovePriceChangeCommandHandled
{
    private readonly Fixture _fixture;
    private readonly Mock<IMessageSession> _mockMessageSession;
    private readonly Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder;
    private Mock<ISystemClockService> _mockSystemClock;

    public WhenApprovePriceChangeCommandHandled()
    {
        _fixture = new Fixture();
        _mockMessageSession = new Mock<IMessageSession>();
        _mockEventBuilder = new Mock<IApprenticeshipEarningsRecalculatedEventBuilder>();
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2019, 12, 1));
    }

    [SetUp]
    public void Setup()
    {
        _mockMessageSession.Reset();
        _mockEventBuilder.Reset();

        _mockEventBuilder.Setup(x => x.Build(It.IsAny<Apprenticeship>())).Returns(new ApprenticeshipEarningsRecalculatedEvent());
    }

    [Test]
    public async Task ThenTheEarningsAreRecalculated()
    {
        // Arrange
        var sut = new ApprovePriceChangeCommandHandler(_mockMessageSession.Object, _mockEventBuilder.Object, _mockSystemClock.Object);
        var command = CreateCommand();

        // Act
        var apprenticeship = await sut.RecalculateEarnings(command);

        // Assert
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private Command.ApprovePriceChangeCommand.ApprovePriceChangeCommand CreateCommand()
    {
        var apprenticeship = _fixture.CreateApprenticeshipEntityModel();
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.Single();

        var priceChangeApprovedEvent = new PriceChangeApprovedEvent
        {
            ApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ApprovedBy = ApprovedBy.Employer,
            ApprovedDate = new DateTime(2019, 12, 1),
            AssessmentPrice = 17000,
            EffectiveFromDate = new DateTime(2019, 11, 1),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            ProviderId = 123,
            TrainingPrice = 3000
        };

        var command = new Command.ApprovePriceChangeCommand.ApprovePriceChangeCommand(apprenticeship, priceChangeApprovedEvent);

        return command;
    }
}