using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
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
    private readonly Mock<IApprenticeshipRepository> _mockRepository = new();

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
        var apprenticeship = BuildApprenticeship();

        SetupMocks();

        var command = BuildCommand(apprenticeship);

        _mockRepository
            .Setup(x => x.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        var sut = new UpdateEnglishAndMathsCommandHandler(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockSystemClock.Object
        );

        // Act
        await sut.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
    }

    private Apprenticeship BuildApprenticeship()
    {
        var apprenticeshipModel = _fixture.Create<LearningModel>();
        apprenticeshipModel.Episodes = [new EpisodeModel(apprenticeshipModel.LearningKey, _fixture.Create<LearningEpisode>(), _fixture.Create<int>(), null){ EarningsProfile = new EarningsProfileModel
        {
            MathsAndEnglishCourses = new List<MathsAndEnglishModel>()
        }}];
        return Apprenticeship.Get(apprenticeshipModel);
    }

    private UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand BuildCommand(Apprenticeship apprenticeship)
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

        return new UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand(apprenticeship.ApprenticeshipKey, request);
    }
}