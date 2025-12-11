using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SaveLearningSupportCommandHandler;

[TestFixture]
public class WhenSaveMathsAndEnglish
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<SaveMathsAndEnglishCommandHandler>> _mockLogger;
    private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
    private Mock<ISystemClockService> _mockSystemClockService;
    private SaveMathsAndEnglishCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SaveMathsAndEnglishCommandHandler>>();
        _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        _mockSystemClockService = new Mock<ISystemClockService>();

        _handler = new SaveMathsAndEnglishCommandHandler(
            _mockLogger.Object,
            _mockApprenticeshipRepository.Object,
            _mockSystemClockService.Object);
    }

    [Test]
    public async Task Handle_ShouldCallRepositoryUpdate_WhenMathsAndEnglishPaymentsAreAdded()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();

        var mathsAndEnglishList = new SaveMathsAndEnglishRequest
        {
            new MathsAndEnglishDetail{ Amount = 500, Course = "A112", StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now },
            new MathsAndEnglishDetail{ Amount = 900, Course = "B114", StartDate = DateTime.Now.AddMonths(-12), EndDate = DateTime.Now.AddMonths(2) }
        };

        var command = new SaveMathsAndEnglishCommand.SaveMathsAndEnglishCommand(learningKey, mathsAndEnglishList);

        var apprenticeshipModel = _fixture.Create<ApprenticeshipModel>();
        apprenticeshipModel.Episodes = new List<EpisodeModel> { _fixture.Create<EpisodeModel>() };

        var apprenticeship = Apprenticeship.Get(apprenticeshipModel);

        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(learningKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(repo => repo.Update(apprenticeship), Times.Once);
    }
}