using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeleteCmd = SFA.DAS.Funding.ApprenticeshipEarnings.Command.DeleteShortCourseLearningCommand.DeleteShortCourseLearningCommand;
using DeleteHandler = SFA.DAS.Funding.ApprenticeshipEarnings.Command.DeleteShortCourseLearningCommand.DeleteShortCourseLearningCommandHandler;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.DeleteShortCourseLearningCommandHandler;

[TestFixture]
public class WhenDeletingShortCourseLearning
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<DeleteHandler>> _mockLogger;
    private Mock<ILearningRepository> _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<DeleteHandler>>();
        _mockRepository = new Mock<ILearningRepository>();
    }

    [Test]
    public async Task Handle_ShouldFetchLearning_UsingLearningKey()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(new DeleteCmd(learningKey));

        _mockRepository.Verify(r => r.GetShortCourseLearning(learningKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPersistUpdatedLearning()
    {
        var learningKey = Guid.NewGuid();
        var learning = BuildShortCourseLearning(learningKey);
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync(learning);

        await BuildSut().Handle(new DeleteCmd(learningKey));

        _mockRepository.Verify(r => r.Update(learning), Times.Once);
    }

    [Test]
    public void Handle_ShouldThrow_WhenLearningNotFound()
    {
        var learningKey = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetShortCourseLearning(learningKey)).ReturnsAsync((ShortCourseLearning?)null);

        var act = async () => await BuildSut().Handle(new DeleteCmd(learningKey));

        act.Should().ThrowAsync<InvalidOperationException>();
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

    private DeleteHandler BuildSut() => new(_mockLogger.Object, _mockRepository.Object);
}
