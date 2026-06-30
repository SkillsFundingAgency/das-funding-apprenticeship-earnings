using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.DeleteShortCourseLearningCommandHandler;

[TestFixture]
public class WhenDeletingShortCourseLearning
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<RemoveShortCourseLearningCommandHandler>> _mockLogger;
    private Mock<ILearningRepository> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<RemoveShortCourseLearningCommandHandler>>();
        _mockRepository = new Mock<ILearningRepository>();
    }

    [Test]
    public async Task Handle_ShouldFetchLearning_UsingLearningKey()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(new RemoveShortCourseLearningCommand.RemoveShortCourseLearningCommand(learningKey, learning.Episodes.Single().EpisodeKey, Guid.NewGuid(), "learner-ref"));

        _mockRepository.Verify(r => r.GetShortCourseLearning(learningKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPersistUpdatedLearning()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(new RemoveShortCourseLearningCommand.RemoveShortCourseLearningCommand(learningKey, learning.Episodes.Single().EpisodeKey, Guid.NewGuid(), "learner-ref"));

        _mockRepository.Verify(r => r.Update(learning), Times.Once);
    }

    [Test]
    public void Handle_ShouldThrow_WhenLearningNotFound()
    {
        var learningKey = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync((ShortCourseLearning?)null);

        var act = async () => await BuildSut().Handle(new RemoveShortCourseLearningCommand.RemoveShortCourseLearningCommand(learningKey, Guid.NewGuid(), Guid.NewGuid(), "learner-ref"));

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task Handle_ShouldSetLearnerValuesOnPayableEarningsUpdatedEvent()
    {
        var learningKey = Guid.NewGuid();
        var learnerKey = Guid.NewGuid();
        const string learnerRef = "learner-ref-updated";
        var learning = BuildShortCourseLearning(learningKey);
        var episodeKey = learning.Episodes.Single().EpisodeKey;

        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(new RemoveShortCourseLearningCommand.RemoveShortCourseLearningCommand(learningKey, episodeKey, learnerKey, learnerRef));

        var @event = learning.FlushEvents().OfType<ShortCoursePayableEarningsUpdatedEvent>().Single();

        @event.LearnerKey.Should().Be(learnerKey);
        @event.LearnerRef.Should().Be(learnerRef);
    }

    private ShortCourseLearning BuildShortCourseLearning(Guid learningKey)
    {
        var episodeEntity = _fixture
            .Build<ShortCourseEpisodeEntity>()
            .With(x => x.StartDate, new DateTime(2021, 1, 1))
            .With(x => x.EndDate, new DateTime(2021, 6, 25))
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.EarningsProfile, _fixture
                .Build<ShortCourseEarningsProfileEntity>()
                .With(x => x.Instalments, new List<ShortCourseInstalmentEntity>())
                .Create())
            .Create();

        var entity = _fixture
            .Build<ShortCourseLearningEntity>()
            .With(x => x.LearningKey, learningKey)
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        return ShortCourseLearning.Get(entity);
    }

    private RemoveShortCourseLearningCommandHandler BuildSut() => new(_mockLogger.Object, _mockRepository.Object);
}
