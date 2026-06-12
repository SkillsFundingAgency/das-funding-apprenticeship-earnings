using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SendShortCoursePayableEarningsToPaymentsCommandTests;

[TestFixture]
public class WhenSendingShortCoursePayableEarningsToPayments
{
    private Fixture _fixture = null!;
    private Mock<ILearningRepository> _mockRepository = null!;
    private Mock<IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder> _mockBuilder = null!;
    private Mock<IMessageSession> _mockMessageSession = null!;
    private PaymentsConfiguration _paymentsConfiguration = null!;
    private SendShortCoursePayableEarningsToPaymentsCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<ILearningRepository>();
        _mockBuilder = new Mock<IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder>();
        _mockMessageSession = new Mock<IMessageSession>();
        _paymentsConfiguration = new PaymentsConfiguration { PaymentsEndpoint = "payments-queue-name" };

        _sut = new SendShortCoursePayableEarningsToPaymentsCommandHandler(
            _mockRepository.Object,
            _mockBuilder.Object,
            _mockMessageSession.Object,
            _paymentsConfiguration,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SendShortCoursePayableEarningsToPaymentsCommandHandler>>());
    }

    [Test]
    public async Task ThenTheShortCourseLearningIsRetrievedFromRepository()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand(shortCoursePayableEarningsUpdatedEvent);

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
        _mockBuilder.Setup(x => x.Build(learning.GetEpisode(), learning, shortCoursePayableEarningsUpdatedEvent.EmployerAccountId, shortCoursePayableEarningsUpdatedEvent.FundingAccountId))
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
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand(shortCoursePayableEarningsUpdatedEvent);

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
        _mockBuilder.Setup(x => x.Build(learning.GetEpisode(), learning, shortCoursePayableEarningsUpdatedEvent.EmployerAccountId, shortCoursePayableEarningsUpdatedEvent.FundingAccountId))
            .Returns(paymentEvent);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockBuilder.Verify(x => x.Build(learning.GetEpisode(), learning, shortCoursePayableEarningsUpdatedEvent.EmployerAccountId, shortCoursePayableEarningsUpdatedEvent.FundingAccountId), Times.Once);
        _mockMessageSession.Verify(x => x.Send(paymentEvent, It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.Is<GrowthAndSkillsPaymentsRecalculatedEvent>(e => e.Command == paymentEvent), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenLearningNotFound_ThrowsException()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand(shortCoursePayableEarningsUpdatedEvent);

        _mockRepository.Setup(x => x.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync((ShortCourseLearning?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Short course learning not found for key: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
    }

    [Test]
    public async Task WhenEpisodeNotFound_ThrowsException()
    {
        // Arrange
        var shortCoursePayableEarningsUpdatedEvent = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand(shortCoursePayableEarningsUpdatedEvent);

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
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Short course episode not found for EpisodeKey: {command.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey} on LearningKey: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
    }
}
