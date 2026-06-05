using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessShortCoursePayableEarningsUpdatedCommandTests;

[TestFixture]
public class WhenProcessingShortCoursePayableEarningsUpdated
{
    private Fixture _fixture = null!;
    private Mock<ILearningRepository> _mockRepository = null!;
    private Mock<IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder> _mockBuilder = null!;
    private Mock<IMessageSession> _mockMessageSession = null!;
    private global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<ILearningRepository>();
        _mockBuilder = new Mock<IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder>();
        _mockMessageSession = new Mock<IMessageSession>();

        _sut = new ProcessShortCoursePayableEarningsUpdatedCommandHandler(
            _mockRepository.Object,
            _mockBuilder.Object,
            _mockMessageSession.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ProcessShortCoursePayableEarningsUpdatedCommandHandler>>());
    }

    [Test]
    public async Task ThenTheShortCourseLearningIsRetrievedFromRepository()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand(shortCoursePayableEarningsUpdatedEvent);

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.Key, shortCoursePayableEarningsUpdatedEvent.EpisodeKey)
            .Create();
        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.LearningKey, shortCoursePayableEarningsUpdatedEvent.LearningKey)
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();
        var learning = ShortCourseLearning.Get(learningEntity);

        _mockRepository.Setup(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        var paymentEvent = new CalculateGrowthAndSkillsPayments();
        _mockBuilder.Setup(x => x.Build(learning.GetEpisode(), learning))
            .Returns(paymentEvent);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey), Times.Once);
    }

    [Test]
    public async Task ThenThePaymentEventIsBuiltAndPublished()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand(shortCoursePayableEarningsUpdatedEvent);

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.Key, shortCoursePayableEarningsUpdatedEvent.EpisodeKey)
            .Create();
        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.LearningKey, shortCoursePayableEarningsUpdatedEvent.LearningKey)
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();
        var learning = ShortCourseLearning.Get(learningEntity);

        _mockRepository.Setup(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        var paymentEvent = new CalculateGrowthAndSkillsPayments();
        _mockBuilder.Setup(x => x.Build(learning.GetEpisode(), learning))
            .Returns(paymentEvent);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockBuilder.Verify(x => x.Build(learning.GetEpisode(), learning), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(paymentEvent, It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenLearningNotFound_ThrowsException()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand(shortCoursePayableEarningsUpdatedEvent);

        _mockRepository.Setup(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync((ShortCourseLearning?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Short course learning not found for key: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
    }

    [Test]
    public async Task WhenEpisodeKeyMismatch_ThrowsException()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand(shortCoursePayableEarningsUpdatedEvent);

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.Key, Guid.NewGuid())
            .Create();
        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.LearningKey, shortCoursePayableEarningsUpdatedEvent.LearningKey)
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();
        var learning = ShortCourseLearning.Get(learningEntity);

        _mockRepository.Setup(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
