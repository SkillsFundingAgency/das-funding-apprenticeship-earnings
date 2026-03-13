using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApproveLearningCommandHandler;

[TestFixture]
public class WhenApprovingLearning
{
    private Fixture _fixture = null!;
    private Mock<ILearningDomainService> _mockDomainService = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockDomainService = new Mock<ILearningDomainService>();
    }

    [Test]
    public async Task ThenGetLearningIsCalledWithTheLearningKey()
    {
        var learning = BuildLearning();
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        _mockDomainService.Verify(x => x.GetLearning(command.LearningKey), Times.Once);
    }

    [Test]
    public async Task ThenTheEpisodeIsApproved()
    {
        var learning = BuildLearning(isApproved: false);
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        var episode = learning.GetFirstEpisode();
        episode.EarningsProfile!.IsApproved.Should().BeTrue();
    }

    [Test]
    public async Task ThenUpdateIsCalledWithTheLearning()
    {
        var learning = BuildLearning();
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        _mockDomainService.Verify(x => x.Update(learning), Times.Once);
    }

    [Test]
    public async Task ThenAnExceptionIsThrownWhenLearningIsNotFound()
    {
        var command = new ApproveLearningCommand.ApproveLearningCommand(Guid.NewGuid());
        _mockDomainService.Setup(x => x.GetLearning(command.LearningKey)).ReturnsAsync((BaseLearning?)null);

        var act = async () => await CreateHandler().Handle(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private ApprenticeshipLearning BuildLearning(bool isApproved = true)
    {
        var episodeEntity = _fixture
            .Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.FundingBandMaximum, int.MaxValue)
            .With(x => x.PeriodsInLearning, new List<ApprenticeshipPeriodInLearningEntity>())
            .With(x => x.Prices, new List<ApprenticeshipEpisodePriceEntity>
            {
                _fixture.Build<ApprenticeshipEpisodePriceEntity>()
                    .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-6))
                    .With(x => x.EndDate, DateTime.UtcNow.AddMonths(6))
                    .Create()
            })
            .With(x => x.EarningsProfile, _fixture
                .Build<ApprenticeshipEarningsProfileEntity>()
                .With(x => x.IsApproved, isApproved)
                .With(x => x.Instalments, new List<ApprenticeshipInstalmentEntity>())
                .With(x => x.ApprenticeshipAdditionalPayments, new List<ApprenticeshipAdditionalPaymentEntity>())
                .With(x => x.EnglishAndMathsCourses, new List<EnglishAndMathsEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture
            .Build<ApprenticeshipLearningEntity>()
            .With(x => x.Episodes, new List<ApprenticeshipEpisodeEntity> { episodeEntity })
            .Create();

        return ApprenticeshipLearning.Get(learningEntity);
    }

    private static Command.ApproveLearningCommand.ApproveLearningCommand BuildCommand(ApprenticeshipLearning learning)
        => new(learning.LearningKey);

    private void SetupDomainService(ApprenticeshipLearning learning)
        => _mockDomainService.Setup(x => x.GetLearning(learning.LearningKey)).ReturnsAsync(learning);

    private Command.ApproveLearningCommand.ApproveLearningCommandHandler CreateHandler()
        => new(_mockDomainService.Object);
}
