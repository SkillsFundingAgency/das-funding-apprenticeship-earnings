using AutoFixture;
using Microsoft.Extensions.Internal;
using Moq;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApproveStartDateChangeCommand;

[TestFixture]
public class WhenApproveStartDateChangeCommandHandled
{
    private readonly Fixture _fixture;
    private readonly Mock<IMessageSession> _mockMessageSession;
    private readonly Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder;
    private Mock<ISystemClock> _mockSystemClock;

    public WhenApproveStartDateChangeCommandHandled()
    {
        _fixture = new Fixture();
        _mockMessageSession = new Mock<IMessageSession>();
        _mockEventBuilder = new Mock<IApprenticeshipEarningsRecalculatedEventBuilder>();
        _mockSystemClock = new Mock<ISystemClock>();
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
        var sut = new ApproveStartDateChangeCommandHandler(_mockMessageSession.Object, _mockEventBuilder.Object, _mockSystemClock.Object);
        var command = CreateCommand();

        // Act
        var apprenticeship = await sut.RecalculateEarnings(command);

        // Assert
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private Command.ApproveStartDateChangeCommand.ApproveStartDateChangeCommand CreateCommand()
    {
        var apprenticeship = _fixture.CreateApprenticeshipEntityModel();
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.Single();

        var apprenticeshipStartDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ActualStartDate = new DateTime(2019, 10, 1),
            PlannedEndDate = currentEpisode.PlannedEndDate,
            EmployerAccountId = currentEpisode.EmployerAccountId,
            ProviderId = 123,
            ApprovedDate = new DateTime(2019, 12, 1),
            ProviderApprovedBy = "",
            EmployerApprovedBy = "",
            Initiator = ""
        };

        var command = new Command.ApproveStartDateChangeCommand.ApproveStartDateChangeCommand(apprenticeship, apprenticeshipStartDateChangedEvent);

        return command;
    }
}