using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateShortCourseOnProgrammeCommandHandler;

[TestFixture]
public class WhenUpdatingShortCourseOnProgramme
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<UpdateShortCourseOnProgrammeCommand.UpdateShortCourseOnProgrammeCommandHandler>> _mockLogger;
    private Mock<ILearningRepository> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UpdateShortCourseOnProgrammeCommand.UpdateShortCourseOnProgrammeCommandHandler>>();
        _mockRepository = new Mock<ILearningRepository>();
    }

    [Test]
    public async Task Handle_ShouldFetchLearning_UsingLearningKey()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        var command = BuildCommand(learningKey, completionDate: new DateTime(2021, 5, 25));

        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(command);

        _mockRepository.Verify(r => r.GetShortCourseLearning(learningKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldUpdateCompletionDate_OnEpisode()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        var completionDate = new DateTime(2021, 5, 25);
        var command = BuildCommand(learningKey, completionDate: completionDate);

        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(command);

        learning.GetEpisode().CompletionDate.Should().Be(completionDate);
    }

    [Test]
    public async Task Handle_ShouldClearCompletionDate_WhenCommandHasNullCompletionDate()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey, existingCompletionDate: new DateTime(2021, 4, 1));
        var command = BuildCommand(learningKey, completionDate: null);

        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(command);

        learning.GetEpisode().CompletionDate.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldPersistUpdatedLearning()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        var command = BuildCommand(learningKey, completionDate: new DateTime(2021, 5, 25));

        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(command);

        _mockRepository.Verify(r => r.Update(learning), Times.Once);
    }

    private ShortCourseLearning BuildShortCourseLearning(Guid learningKey, DateTime? existingCompletionDate = null)
    {
        var episodeEntity = _fixture
            .Build<ShortCourseEpisodeEntity>()
            .With(x => x.StartDate, new DateTime(2021, 1, 1))
            .With(x => x.EndDate, new DateTime(2021, 6, 25))
            .With(x => x.CompletionDate, existingCompletionDate)
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

    private UpdateShortCourseOnProgrammeCommand.UpdateShortCourseOnProgrammeCommandHandler BuildSut() =>
        new(_mockLogger.Object, _mockRepository.Object);

    private static UpdateShortCourseOnProgrammeCommand.UpdateShortCourseOnProgrammeCommand BuildCommand(
        Guid learningKey, DateTime? completionDate = null) =>
        new(learningKey, new UpdateShortCourseOnProgrammeCommand.UpdateShortCourseOnProgrammeRequest
        {
            CompletionDate = completionDate,
            Milestones = new List<Milestone>()
        });
}
