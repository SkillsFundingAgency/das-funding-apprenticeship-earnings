using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests;

public class CreateApprenticeshipCommandHandlerTests
{
    private ICreateApprenticeshipCommandHandler _sut = null!;
    private readonly Mock<IApprenticeshipFactory> _apprenticeshipFactoryMock = new();
    private readonly Mock<IMessageSession> _messageSessionMock = new();
    private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder = new EarningsGeneratedEventBuilder();
    private readonly Fixture _fixture = new();

    [SetUp]
    public void Setup()
    {
        _sut = new CreateApprenticeshipCommandHandler(_apprenticeshipFactoryMock.Object,
            _messageSessionMock.Object, _earningsGeneratedEventBuilder);
    }

    [Test]
    public async Task Create_creates_Apprenticeship_domain_model()
    {
        var result = await ArrangeAndAct();

        // Assert
        result.actual.Should().BeEquivalentTo(result.expected);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_ApprenticeshipKey()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.ApprenticeshipKey == expected.ApprenticeshipKey),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_Uln()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.Uln == expected.Uln),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_CommitmentId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.CommitmentId == expected.CommitmentId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_EmployerId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.EmployerId == expected.EmployerId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_ProviderId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.ProviderId == expected.ProviderId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_TransferSenderEmployerId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.TransferSenderEmployerId == expected.TransferSenderEmployerId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_AgreedPrice()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.AgreedPrice == expected.AgreedPrice),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_StartDate()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.StartDate == expected.StartDate),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_ActualEndDate()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.ActualEndDate == expected.ActualEndDate),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_TrainingCode()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.TrainingCode == expected.TrainingCode),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_EmployerType()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.EmployerType == expected.EmployerType),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_EmploymentStatus()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.EmploymentStatus == expected.EmploymentStatus),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_TrainingStatus()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.TrainingStatus == expected.TrainingStatus),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test, Ignore("Not mapped")]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_CoInvestmentPercentage()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.CoInvestmentPercentage == expected.CoInvestmentPercentage),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_DeliveryPeriods()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => 
                FluentVerifier.VerifyAssertion(() => actual.DeliveryPeriods.Should().BeEquivalentTo(expected.DeliveryPeriods, ""))),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_EmployerAccountId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.EmployerAccountId == expected.EmployerAccountId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_PlannedEndDate()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.PlannedEndDate == expected.PlannedEndDate),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    [Test]
    public async Task Create_publishes_EarningsGeneratedEvent_with_correct_ApprovalsApprenticeshipId()
    {
        // Arrange
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(
            It.Is<EarningsGeneratedEvent>(actual => actual.ApprovalsApprenticeshipId == expected.ApprovalsApprenticeshipId),
            It.IsAny<PublishOptions>()),
            Times.Once);
    }

    private async Task<(Apprenticeship actual, Apprenticeship expected)> ArrangeAndAct()
    {
        // Arrange
        var input = _fixture.Create<CreateApprenticeshipCommand.CreateApprenticeshipCommand>();
        input.ApprenticeshipEntity.PlannedEndDate = input.ApprenticeshipEntity.ActualStartDate.AddMonths(12);
        var apprenticeship = new ApprenticeshipFactory().CreateNew(input.ApprenticeshipEntity);
        apprenticeship.CalculateEarnings();
        _apprenticeshipFactoryMock.Setup(_ => _.CreateNew(input.ApprenticeshipEntity)).Returns(apprenticeship);

        // Act
        var result = await _sut.Create(input);

        return (result, apprenticeship);
    }
}