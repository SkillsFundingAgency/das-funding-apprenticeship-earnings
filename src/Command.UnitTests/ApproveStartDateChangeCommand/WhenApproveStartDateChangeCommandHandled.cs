using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
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

    public WhenApproveStartDateChangeCommandHandled()
    {
        _fixture = new Fixture();
        _mockMessageSession = new Mock<IMessageSession>();
        _mockEventBuilder = new Mock<IApprenticeshipEarningsRecalculatedEventBuilder>();
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
        var sut = new ApproveStartDateChangeCommandHandler(_mockMessageSession.Object, _mockEventBuilder.Object);
        var command = CreateCommand();

        // Act
        var apprenticeship = await sut.RecalculateEarnings(command);

        // Assert
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private Command.ApproveStartDateChangeCommand.ApproveStartDateChangeCommand CreateCommand()
    {

        var apprenticeship = _fixture.Create<ApprenticeshipEntityModel>();
        apprenticeship.ActualStartDate = new DateTime(2019, 09, 01);
        apprenticeship.PlannedEndDate = new DateTime(2020, 1, 1);
        apprenticeship.AgeAtStartOfApprenticeship = 21;
        apprenticeship.AgreedPrice = 10000;
        apprenticeship.FundingBandMaximum = 20000;
        apprenticeship.EarningsProfile.AdjustedPrice = 10000;
        apprenticeship.EarningsProfile.CompletionPayment = 4000;
        apprenticeship.EarningsProfile.Instalments = new List<InstalmentEntityModel>
        {
            new() { AcademicYear = 1920, DeliveryPeriod = 2, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 3, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 4, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 5, Amount = 2500}
        };

        var apprenticeshipStartDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ActualStartDate = new DateTime(2019, 10, 1),
            PlannedEndDate = apprenticeship.PlannedEndDate,
            EmployerAccountId = apprenticeship.EmployerAccountId,
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