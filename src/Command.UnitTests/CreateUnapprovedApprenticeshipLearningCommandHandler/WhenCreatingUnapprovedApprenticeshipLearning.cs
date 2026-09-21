using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.CreateUnapprovedApprenticeshipLearningCommandHandler;

[TestFixture]
public class WhenCreatingUnapprovedApprenticeshipLearning
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommandHandler>> _logger = null!;
    private Mock<ILearningRepository> _repository = null!;
    private Mock<ISystemClockService> _systemClock = null!;
    private ILearningFactory _learningFactory = null!;
    private ApprenticeshipOptInConfiguration _apprenticeshipOptInConfiguration = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommandHandler>>();
        _repository = new Mock<ILearningRepository>();
        _systemClock = new Mock<ISystemClockService>();
        _learningFactory = new LearningFactory();
        _apprenticeshipOptInConfiguration = new ApprenticeshipOptInConfiguration
        {
            StartDate = new DateTime(2020, 1, 1),
            Providers = [12345678]
        };

        _systemClock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
    }

    [Test]
    public async Task Then_New_Draft_Learning_Is_Added_When_Learning_Does_Not_Exist()
    {
        var request = BuildRequest();
        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request)
        {
            Request =
            {
                IsNewApprenticeshipLearner = true
            }
        };

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning?)null);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Add(It.Is<ApprenticeshipLearning>(l =>
            l.HasEpisode(request.EpisodeKey) &&
            l.GetEpisode(request.EpisodeKey).EarningsProfile != null &&
            !l.GetEpisode(request.EpisodeKey).EarningsProfile!.IsApproved)), Times.Once);
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Once);
    }

    [Test]
    public async Task Then_Nothing_Happens_When_Learning_Does_Not_Exist_And_IsNotNewApprenticeshipLearner()
    {
        var request = BuildRequest();
        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request)
        {
            Request =
            {
                IsNewApprenticeshipLearner = false
            }
        };

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning?)null);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
        _repository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [Test]
    public async Task Then_Existing_Learning_Is_Updated_When_Episode_Exists()
    {
        var request = BuildRequest();
        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request);
        var existingLearning = _learningFactory.CreateNewUnapprovedApprenticeship(request, 10000);

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync(existingLearning);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Update(It.Is<ApprenticeshipLearning>(l =>
            l.HasEpisode(request.EpisodeKey) &&
            l.GetEpisode(request.EpisodeKey).EarningsProfile != null &&
            !l.GetEpisode(request.EpisodeKey).EarningsProfile!.IsApproved)), Times.Once);
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [Test]
    public async Task Then_New_Episode_Is_Added_When_Learning_Exists_But_Episode_Is_New()
    {
        var existingRequest = BuildRequest();
        var existingLearning = _learningFactory.CreateNewUnapprovedApprenticeship(existingRequest, 10000);

        var request = BuildRequest();
        request.LearningKey = existingRequest.LearningKey;
        request.EpisodeKey = Guid.NewGuid();

        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request);

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync(existingLearning);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Update(It.Is<ApprenticeshipLearning>(l =>
            l.HasEpisode(existingRequest.EpisodeKey) &&
            l.HasEpisode(request.EpisodeKey))), Times.Once);
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [TestCase(false, true, TestName = "Then_New_Episode_Is_Not_Added_When_OptIn_Criteria_Not_Met(StartDate not opted in)")]
    [TestCase(true, false, TestName = "Then_New_Episode_Is_Not_Added_When_OptIn_Criteria_Not_Met(Provider not opted in)")]
    public async Task Then_New_Episode_Is_Not_Added_When_OptIn_Criteria_Not_Met(bool startDateOptedIn, bool providerOptedIn)
    {
        _apprenticeshipOptInConfiguration.StartDate = startDateOptedIn ? new DateTime(2020, 1, 1) : new DateTime(2030, 1, 1);
        _apprenticeshipOptInConfiguration.Providers = providerOptedIn ? [12345678] : [];

        var existingRequest = BuildRequest();
        var existingLearning = _learningFactory.CreateNewUnapprovedApprenticeship(existingRequest, 10000);

        var request = BuildRequest();
        request.LearningKey = existingRequest.LearningKey;
        request.EpisodeKey = Guid.NewGuid();

        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request);

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync(existingLearning);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [TestCase(false, true, TestName = "Then_New_Draft_Learning_Is_Not_Added_When_OptIn_Criteria_Not_Met(StartDate not opted in)")]
    [TestCase(true, false, TestName = "Then_New_Draft_Learning_Is_Not_Added_When_OptIn_Criteria_Not_Met(Provider not opted in)")]
    public async Task Then_New_Draft_Learning_Is_Not_Added_When_OptIn_Criteria_Not_Met(bool startDateOptedIn, bool providerOptedIn)
    {
        _apprenticeshipOptInConfiguration.StartDate = startDateOptedIn ? new DateTime(2020, 1, 1) : new DateTime(2030, 1, 1);
        _apprenticeshipOptInConfiguration.Providers = providerOptedIn ? [12345678] : [];

        var request = BuildRequest();
        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request);

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning?)null);

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
        _repository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    [TestCase(false, true, TestName = "Then_Existing_Unapproved_Episode_Is_Removed_When_OptIn_Criteria_Not_Met(StartDate not opted in)")]
    [TestCase(true, false, TestName = "Then_Existing_Unapproved_Episode_Is_Removed_When_OptIn_Criteria_Not_Met(Provider not opted in)")]
    public async Task Then_Existing_Unapproved_Episode_Is_Removed_When_OptIn_Criteria_Not_Met(bool startDateOptedIn, bool providerOptedIn)
    {
        var request = BuildRequest();
        var command = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommand(request);
        var existingLearning = _learningFactory.CreateNewUnapprovedApprenticeship(request, 10000);
        existingLearning.Calculate(_systemClock.Object, "{}", request.EpisodeKey, initialGenerationIsApproved: false);

        _repository
            .Setup(x => x.GetApprenticeshipLearning(request.LearningKey))
            .ReturnsAsync(existingLearning);

        _apprenticeshipOptInConfiguration.StartDate = startDateOptedIn ? new DateTime(2020, 1, 1) : new DateTime(2030, 1, 1);
        _apprenticeshipOptInConfiguration.Providers = providerOptedIn ? [12345678] : [];

        var sut = BuildHandler();

        await sut.Handle(command, CancellationToken.None);

        _repository.Verify(x => x.Update(It.Is<ApprenticeshipLearning>(l =>
            l.GetEpisode(request.EpisodeKey).IsRemoved)), Times.Once);
        _repository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    private SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommandHandler BuildHandler()
    {
        return new SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand.CreateUnapprovedApprenticeshipLearningCommandHandler(
            _logger.Object,
            _learningFactory,
            _repository.Object,
            _systemClock.Object,
            _apprenticeshipOptInConfiguration);
    }

    private CreateUnapprovedApprenticeshipLearningRequest BuildRequest()
    {
        var startDate = new DateTime(2025, 8, 1);
        var endDate = new DateTime(2027, 7, 31);

        return new CreateUnapprovedApprenticeshipLearningRequest
        {
            LearningKey = Guid.NewGuid(),
            EpisodeKey = Guid.NewGuid(),
            ApprovalsApprenticeshipId = _fixture.Create<long>(),
            Learner = new DraftApprenticeshipLearner
            {
                DateOfBirth = new DateTime(2005, 1, 1),
                Uln = "1234567890",
                Care = new DraftCare
                {
                    HasEHCP = false,
                    IsCareLeaver = false,
                    CareLeaverEmployerConsentGiven = false
                }
            },
            OnProgramme = new DraftApprenticeshipOnProgramme
            {
                TrainingCode = "123",
                Ukprn = 12345678,
                EmployerAccountId = 100,
                FundingEmployerAccountId = 200,
                LegalEntityName = "Test Employer",
                EmployerType = EmployerType.Levy,
                FundingBandMaximum = 10000
            },
            Prices =
            [
                new LearningEpisodePrice
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = 12000,
                    TrainingPrice = 9600,
                    EndPointAssessmentPrice = 2400
                }
            ],
            PeriodsInLearning =
            [
                new ApprenticeshipPeriodInLearningItem
                {
                    StartDate = startDate,
                    EndDate = null,
                    OriginalExpectedEndDate = endDate
                }
            ],
            EnglishAndMaths =
            [
                new DraftEnglishAndMathsItem
                {
                    Course = "English",
                    LearnAimRef = "ENG001",
                    StartDate = startDate,
                    EndDate = endDate,
                    Amount = 471,
                    PeriodsInLearning =
                    [
                        new ApprenticeshipPeriodInLearningItem
                        {
                            StartDate = startDate,
                            EndDate = null,
                            OriginalExpectedEndDate = endDate
                        }
                    ]
                }
            ],
            LearningSupport =
            [
                new LearningSupportItem
                {
                    StartDate = startDate,
                    EndDate = endDate
                }
            ]
        };
    }
}
