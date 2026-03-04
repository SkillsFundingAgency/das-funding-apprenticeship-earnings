using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SaveLearningSupportCommandHandler;

[TestFixture]
public class WhenUpdatingEnglishAndMaths
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<UpdateEnglishAndMathsCommandHandler>> _mockLogger;
    private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
    private Mock<ISystemClockService> _mockSystemClockService;
    private UpdateEnglishAndMathsCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<UpdateEnglishAndMathsCommandHandler>>();
        _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        _mockSystemClockService = new Mock<ISystemClockService>();

        _handler = new UpdateEnglishAndMathsCommandHandler(
            _mockLogger.Object,
            _mockApprenticeshipRepository.Object,
            _mockSystemClockService.Object);
    }

    [Test]
    public async Task Handle_ShouldCallRepositoryUpdate_WhenMathsAndEnglishPaymentsAreAdded()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();

        var mathsAndEnglishList = new UpdateEnglishAndMathsRequest
        {
            EnglishAndMaths = new List<EnglishAndMathsItem>()
            {
                new EnglishAndMathsItem{ Amount = 500, Course = "A112", StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now },
                new EnglishAndMathsItem{ Amount = 900, Course = "B114", StartDate = DateTime.Now.AddMonths(-12), EndDate = DateTime.Now.AddMonths(2) }
            }
        };

        var command = new UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand(learningKey, mathsAndEnglishList);

        var learningModel = _fixture.Create<LearningModel>();
        learningModel.Episodes = new List<EpisodeModel> { _fixture.Create<EpisodeModel>() };

        var apprenticeship = Apprenticeship.Get(learningModel);

        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(learningKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(repo => repo.Update(apprenticeship), Times.Once);
    }
}