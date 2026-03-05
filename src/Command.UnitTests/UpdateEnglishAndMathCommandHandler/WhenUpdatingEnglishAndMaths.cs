using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateEnglishAndMathCommandHandler;

[TestFixture]
public class WhenUpdatingEnglishAndMaths
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ILogger<UpdateEnglishAndMathsCommandHandler>> _mockLogger = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<ILearningRepository> _mockRepository = new();

    private void SetupMocks()
    {
        _mockRepository.Reset();

        _mockSystemClock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2024, 12, 1));
    }

    [Test]
    public async Task ThenTheMathsAndEnglishCourseIsAdded()
    {
        // Arrange
        var learningDomainModel = BuildLearning();

        SetupMocks();

        var command = BuildCommand(learningDomainModel);

        _mockRepository
            .Setup(x => x.GetApprenticeshipLearning(command.LearningKey))
            .ReturnsAsync(learningDomainModel);

        var sut = new UpdateEnglishAndMathsCommandHandler(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockSystemClock.Object
        );

        // Act
        await sut.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.GetApprenticeshipLearning(command.LearningKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Once);
    }

    private ApprenticeshipLearning BuildLearning()
    {
        var learningEntity = _fixture.Create<ApprenticeshipLearningEntity>();
        learningEntity.Episodes = [new ApprenticeshipEpisodeEntity(learningEntity.LearningKey, _fixture.Create<LearningEpisode>(), _fixture.Create<int>(), null){ EarningsProfile = new ApprenticeshipEarningsProfileEntity
        {
            EnglishAndMathsCourses = new List<EnglishAndMathsEntity>()
        }}];
        return ApprenticeshipLearning.Get(learningEntity);
    }

    private UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand BuildCommand(ApprenticeshipLearning apprenticeship)
    {
        var request = new UpdateEnglishAndMathsRequest
        {
            EnglishAndMaths = new List<EnglishAndMathsItem>
            {
                new()
                {
                    Amount = 1500m,
                    Course = "Standard Code",
                    LearnAimRef = "Maths Level 2",
                    StartDate = new DateTime(2024, 01, 01),
                    EndDate = new DateTime(2024, 12, 31)
                }
            }

        };

        return new UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand(apprenticeship.LearningKey, request);
    }
}