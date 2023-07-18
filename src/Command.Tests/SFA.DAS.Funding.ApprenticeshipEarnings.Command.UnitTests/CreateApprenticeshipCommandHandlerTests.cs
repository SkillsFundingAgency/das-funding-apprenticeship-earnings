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
    public async Task Create_publishes_EarningsGeneratedEvent()
    {
        var result = await ArrangeAndAct();

        // Assert
        var expected = _earningsGeneratedEventBuilder.Build(result.actual);
        _messageSessionMock.Verify(ms => ms.Publish(It.Is<EarningsGeneratedEvent>(actual => 
               FluentVerifier.VerifyFluentAssertion(() => actual.Should().BeEquivalentTo(expected, "event data mismatch"))),It.IsAny<PublishOptions>()), Times.Once);
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