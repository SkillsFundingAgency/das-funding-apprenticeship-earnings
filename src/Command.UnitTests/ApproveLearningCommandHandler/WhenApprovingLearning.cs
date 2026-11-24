using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApproveLearningCommandHandler;

[TestFixture]
public class WhenApprovingLearning
{
    private Fixture _fixture = null!;
    private Mock<ILearningDomainService> _mockDomainService = null!;
    private Mock<ILogger<Command.ApproveLearningCommand.ApproveLearningCommandHandler>> _mockLogger = null!;
    private Mock<ISystemClockService> _mockSystemClock = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockDomainService = new Mock<ILearningDomainService>();
        _mockLogger = new Mock<ILogger<Command.ApproveLearningCommand.ApproveLearningCommandHandler>>();
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 11, 1));
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

        learning.Episodes.Single().EarningsProfile!.IsApproved.Should().BeTrue();
    }

    [Test]
    public async Task ThenTheApprovalsApprenticeshipIdIsUpdatedForShortCourseLearning()
    {
        var learning = BuildLearning(isApproved: false);
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        learning.ApprovalsApprenticeshipId.Should().Be(command.ApprovalsApprenticeshipId);
    }

    [Test]
    public async Task ThenTheEmployerTypeIsUpdatedForShortCourseLearning()
    {
        var learning = BuildLearning(isApproved: false);
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        learning.Episodes.Single().EmployerType.Should().Be(command.EmployerType);
    }

    [Test]
    public async Task ThenTheApprovalsApprenticeshipIdIsUpdatedForApprenticeshipLearning()
    {
        var learning = BuildApprenticeshipLearning();
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        learning.ApprovalsApprenticeshipId.Should().Be(command.ApprovalsApprenticeshipId);
    }

    [Test]
    public async Task ThenTheEmployerTypeIsUpdatedForApprenticeshipLearning()
    {
        var learning = BuildApprenticeshipLearning();
        var command = BuildCommand(learning);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        learning.Episodes.Single().EmployerType.Should().Be(command.EmployerType);
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
    public async Task ThenNoExceptionIsThrownWhenLearningIsNotFound()
    {
        var command = new ApproveLearningCommand.ApproveLearningCommand(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<Types.EmployerType>());
        _mockDomainService.Setup(x => x.GetLearning(command.LearningKey)).ReturnsAsync((BaseLearning?)null);

        var act = async () => await CreateHandler().Handle(command);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task AndLearningIsNotFound_ThenUpdateIsNeverCalled()
    {
        var command = new ApproveLearningCommand.ApproveLearningCommand(Guid.NewGuid(), Guid.NewGuid(), _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<Types.EmployerType>());
        _mockDomainService.Setup(x => x.GetLearning(command.LearningKey)).ReturnsAsync((BaseLearning?)null);

        await CreateHandler().Handle(command);

        _mockDomainService.Verify(x => x.Update(It.IsAny<BaseLearning>()), Times.Never);
    }

    private ShortCourseLearning BuildLearning(bool isApproved = true)
    {
        var episodeEntity = _fixture
            .Build<ShortCourseEpisodeEntity>()
            .With(x => x.StartDate, new DateTime(2021, 1, 1))
            .With(x => x.EndDate, new DateTime(2021, 6, 25))
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.EarningsProfile, _fixture
                .Build<ShortCourseEarningsProfileEntity>()
                .With(x => x.IsApproved, isApproved)
                .With(x => x.Instalments, new List<ShortCourseInstalmentEntity>())
                .Create())
            .Create();

        var entity = _fixture
            .Build<ShortCourseLearningEntity>()
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        return ShortCourseLearning.Get(entity);
    }

    private ApprenticeshipLearning BuildApprenticeshipLearning()
    {
        var episodeEntity = _fixture
            .Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.EarningsProfile, _fixture
                .Build<ApprenticeshipEarningsProfileEntity>()
                .With(x => x.IsApproved, false)
                .With(x => x.Instalments, new List<ApprenticeshipInstalmentEntity>())
                .Create())
            .Create();

        var entity = _fixture
            .Build<ApprenticeshipLearningEntity>()
            .With(x => x.Episodes, new List<ApprenticeshipEpisodeEntity> { episodeEntity })
            .Create();

        return ApprenticeshipLearning.Get(entity);
    }

    private ApproveLearningCommand.ApproveLearningCommand BuildCommand(ShortCourseLearning learning)
        => new(learning.LearningKey, learning.Episodes.Single().EpisodeKey, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<Types.EmployerType>());

    private ApproveLearningCommand.ApproveLearningCommand BuildCommand(ApprenticeshipLearning learning)
        => new(learning.LearningKey, learning.Episodes.Single().EpisodeKey, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<Types.EmployerType>());

    private void SetupDomainService(ShortCourseLearning learning)
        => _mockDomainService.Setup(x => x.GetLearning(learning.LearningKey)).ReturnsAsync(learning);

    private void SetupDomainService(ApprenticeshipLearning learning)
        => _mockDomainService.Setup(x => x.GetLearning(learning.LearningKey)).ReturnsAsync(learning);

    [TestCase(Types.EmployerType.NonLevy, 1)]
    [TestCase(Types.EmployerType.Levy, 0)]
    public async Task ThenTheHiringGrantIsGeneratedOnlyForNonLevyEmployers(Types.EmployerType employerType, int expectedGrants)
    {
        var learning = BuildApprenticeshipLearningForHiringGrant(usingEarnAndPayCalc: true);
        var command = BuildCommand(learning, employerType);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        learning.Episodes.Single().EarningsProfile!.AdditionalPayments
            .Count(x => x.AdditionalPaymentType == InstalmentTypes.EmployerHiringGrant).Should().Be(expectedGrants * 2);
    }

    [Test]
    public async Task ThenTheHiringGrantIsGeneratedForAnEpisodeNotUsingTheEarnAndPayCalculation()
    {
        var learning = BuildApprenticeshipLearningForHiringGrant(usingEarnAndPayCalc: false);
        var command = BuildCommand(learning, Types.EmployerType.NonLevy);
        SetupDomainService(learning);

        await CreateHandler().Handle(command);

        var episode = learning.Episodes.Single();
        episode.UsingEarnAndPayCalc.Should().BeFalse();
        episode.EarningsProfile!.IsApproved.Should().BeTrue();
        episode.EarningsProfile.Instalments.Should().BeEmpty();
        var grants = episode.EarningsProfile.AdditionalPayments.Where(x => x.AdditionalPaymentType == InstalmentTypes.EmployerHiringGrant).OrderBy(x => x.DueDate).ToList();
        grants.Select(x => x.DueDate).Should().Equal(new DateTime(2026, 12, 29), new DateTime(2027, 9, 30));
        grants.Should().OnlyContain(x => x.Amount == 1000m);
    }

    private ApprenticeshipLearning BuildApprenticeshipLearningForHiringGrant(bool usingEarnAndPayCalc)
    {
        var episodeKey = Guid.NewGuid();
        var episodeEntity = new ApprenticeshipEpisodeEntity
        {
            Key = episodeKey,
            UsingEarnAndPayCalc = usingEarnAndPayCalc,
            Prices = [new ApprenticeshipEpisodePriceEntity { Key = Guid.NewGuid(), EpisodeKey = episodeKey, StartDate = new DateTime(2026, 10, 1), EndDate = new DateTime(2028, 9, 30), AgreedPrice = 12000 }],
            EarningsProfile = new ApprenticeshipEarningsProfileEntity
            {
                EarningsProfileId = Guid.NewGuid(),
                EpisodeKey = episodeKey,
                IsApproved = false,
                CalculationData = "{}",
                Instalments = [],
                ApprenticeshipAdditionalPayments = [],
                EnglishAndMathsCourses = []
            }
        };

        return ApprenticeshipLearning.Get(new ApprenticeshipLearningEntity
        {
            LearningKey = Guid.NewGuid(),
            DateOfBirth = new DateTime(2008, 1, 1),
            Episodes = [episodeEntity]
        });
    }

    private ApproveLearningCommand.ApproveLearningCommand BuildCommand(ApprenticeshipLearning learning, Types.EmployerType employerType)
        => new(learning.LearningKey, learning.Episodes.Single().EpisodeKey, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), employerType);

    private Command.ApproveLearningCommand.ApproveLearningCommandHandler CreateHandler()
        => new(_mockDomainService.Object, _mockLogger.Object, _mockSystemClock.Object);
}