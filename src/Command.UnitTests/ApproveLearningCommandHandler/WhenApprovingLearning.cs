using AutoFixture;
using FluentAssertions;
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

        learning.Episodes.Single().EarningsProfile!.IsApproved.Should().BeTrue();
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
        var command = new ApproveLearningCommand.ApproveLearningCommand(Guid.NewGuid(), Guid.NewGuid());
        _mockDomainService.Setup(x => x.GetLearning(command.LearningKey)).ReturnsAsync((BaseLearning?)null);

        var act = async () => await CreateHandler().Handle(command);

        await act.Should().ThrowAsync<InvalidOperationException>();
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

    private static ApproveLearningCommand.ApproveLearningCommand BuildCommand(ShortCourseLearning learning)
        => new(learning.LearningKey, learning.Episodes.Single().EpisodeKey);

    private void SetupDomainService(ShortCourseLearning learning)
        => _mockDomainService.Setup(x => x.GetLearning(learning.LearningKey)).ReturnsAsync(learning);

    private Command.ApproveLearningCommand.ApproveLearningCommandHandler CreateHandler()
        => new(_mockDomainService.Object);
}
