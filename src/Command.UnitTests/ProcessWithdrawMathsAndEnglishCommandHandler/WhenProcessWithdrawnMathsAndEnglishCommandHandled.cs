using AutoFixture;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessWithdrawMathsAndEnglishCommandHandler;

[TestFixture]
public class WhenProcessWithdrawnMathsAndEnglishCommandHandled
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
        _mockRepository.Reset();

        _mockEventBuilder.Setup(x => x.Build(It.IsAny<Apprenticeship>()))
            .Returns(new ApprenticeshipEarningsRecalculatedEvent());

        _mockSystemClock.Setup(x => x.UtcNow)
            .Returns(new DateTime(2024, 12, 1));
    }

    [Test]
    public async Task ThenTheMathsAndEnglishCourseIsWithdrawnAndEventIsPublished()
    {
        // Arrange
        var apprenticeship = BuildApprenticeship();

        SetupMocks();

        var command = BuildCommand(apprenticeship);

        _mockRepository
            .Setup(x => x.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        var sut = new ProcessWithdrawnMathsAndEnglishCommandHandler(
            _mockRepository.Object,
            _mockMessageSession.Object,
            _mockEventBuilder.Object,
            _mockSystemClock.Object
        );

        // Act
        await sut.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
        _mockEventBuilder.Verify(x => x.Build(It.IsAny<Apprenticeship>()), Times.Once);
        _mockMessageSession.Verify(
            x => x.Publish(It.IsAny<ApprenticeshipEarningsRecalculatedEvent>(),
                It.IsAny<PublishOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private Apprenticeship BuildApprenticeship()
    {
        var apprenticeshipModel = _fixture.Create<ApprenticeshipModel>();
        apprenticeshipModel.Episodes = [new EpisodeModel(apprenticeshipModel.Key, _fixture.Create<LearningEpisode>()){ EarningsProfile = new EarningsProfileModel
        {
            MathsAndEnglishCourses = new List<MathsAndEnglishModel>
            {
                _fixture.Build<MathsAndEnglishModel>().With(x => 
                    x.Instalments, 
                    _fixture.Build<MathsAndEnglishInstalmentModel>().With(x => 
                        x.Type, MathsAndEnglishInstalmentType.Regular.ToString)
                        .CreateMany().ToList())
                    .Create()
            }
        }}];
        return Apprenticeship.Get(apprenticeshipModel);
    }

    private ProcessWithdrawnMathsAndEnglishCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var request = new MathsAndEnglishWithdrawRequest
        {
            Course = apprenticeship.ApprenticeshipEpisodes.FirstOrDefault().EarningsProfile.MathsAndEnglishCourses.FirstOrDefault().Course,
            WithdrawalDate = _fixture.Create<DateTime>()
        };

        return new ProcessWithdrawnMathsAndEnglishCommand(apprenticeship.ApprenticeshipKey, request);
    }
}