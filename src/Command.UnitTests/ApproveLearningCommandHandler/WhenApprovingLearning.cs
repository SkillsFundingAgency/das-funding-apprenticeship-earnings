using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApproveLearningCommandHandler;

[TestFixture]
public class WhenApprovingLearning
{
    private Fixture _fixture = null!;
    private Mock<ILearningDomainService> _mockDomainService = null!;
    private Mock<ILogger<Command.ApproveLearningCommand.ApproveLearningCommandHandler>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockDomainService = new Mock<ILearningDomainService>();
        _mockLogger = new Mock<ILogger<Command.ApproveLearningCommand.ApproveLearningCommandHandler>>();
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

    private ApproveLearningCommand.ApproveLearningCommand BuildCommand(ShortCourseLearning learning)
        => new(learning.LearningKey, learning.Episodes.Single().EpisodeKey, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<Types.EmployerType>());

    private void SetupDomainService(ShortCourseLearning learning)
        => _mockDomainService.Setup(x => x.GetLearning(learning.LearningKey)).ReturnsAsync(learning);

    private Command.ApproveLearningCommand.ApproveLearningCommandHandler CreateHandler()
        => new(_mockDomainService.Object, _mockLogger.Object);
}