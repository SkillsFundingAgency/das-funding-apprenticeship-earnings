using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SendApprenticeshipPayableEarningsToPaymentsCommandTests;

[TestFixture]
public class WhenSendingApprenticeshipPayableEarningsToPayments
{
    private Fixture _fixture = null!;
    private Mock<ILearningRepository> _mockRepository = null!;
    private Mock<IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder> _mockBuilder = null!;
    private Mock<IMessageSession> _mockMessageSession = null!;
    private PaymentsConfiguration _paymentsConfiguration = null!;
    private SendApprenticeshipPayableEarningsToPaymentsCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<ILearningRepository>();
        _mockBuilder = new Mock<IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder>();
        _mockMessageSession = new Mock<IMessageSession>();
        _paymentsConfiguration = new PaymentsConfiguration { PaymentsEndpoint = "payments-queue-name" };

        _sut = new SendApprenticeshipPayableEarningsToPaymentsCommandHandler(
            _mockRepository.Object,
            _mockBuilder.Object,
            _mockMessageSession.Object,
            _paymentsConfiguration,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SendApprenticeshipPayableEarningsToPaymentsCommandHandler>>());
    }

    private (ApprenticeshipLearning learning, ApprenticeshipEpisodeEntity episodeEntity) BuildLearning(
        Guid learningKey, Guid episodeKey, List<EnglishAndMathsEntity>? englishAndMathsCourses = null)
    {
        var episodeEntity = _fixture.Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.Key, episodeKey)
            .With(x => x.LearningKey, learningKey)
            .With(x => x.EarningsProfile, _fixture.Build<ApprenticeshipEarningsProfileEntity>()
                .With(x => x.Instalments, new List<ApprenticeshipInstalmentEntity>())
                .With(x => x.EnglishAndMathsCourses, englishAndMathsCourses ?? new List<EnglishAndMathsEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ApprenticeshipLearningEntity>()
            .With(x => x.LearningKey, learningKey)
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ApprenticeshipEpisodeEntity> { episodeEntity })
            .Create();

        return (ApprenticeshipLearning.Get(learningEntity), episodeEntity);
    }

    [Test]
    public async Task ThenTheApprenticeshipLearningIsRetrievedFromRepository()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        var (learning, _) = BuildLearning(apprenticeshipPayableEarningsUpdatedEvent.LearningKey, apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockRepository.Verify(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey), Times.Once);
    }

    [Test]
    public async Task ThenThePaymentEventIsBuiltAndSentAndRecalculatedEventPublished()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        var (learning, _) = BuildLearning(apprenticeshipPayableEarningsUpdatedEvent.LearningKey, apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey);
        var episode = learning.GetEpisode(apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        var paymentEvent = new CalculateGrowthAndSkillsPayments();
        _mockBuilder.Setup(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                apprenticeshipPayableEarningsUpdatedEvent.EmployerAccountId,
                apprenticeshipPayableEarningsUpdatedEvent.FundingAccountId,
                apprenticeshipPayableEarningsUpdatedEvent.LearnerKey,
                apprenticeshipPayableEarningsUpdatedEvent.LearnerRef))
            .Returns(paymentEvent);

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                apprenticeshipPayableEarningsUpdatedEvent.EmployerAccountId,
                apprenticeshipPayableEarningsUpdatedEvent.FundingAccountId,
                apprenticeshipPayableEarningsUpdatedEvent.LearnerKey,
                apprenticeshipPayableEarningsUpdatedEvent.LearnerRef), Times.Once);
        _mockMessageSession.Verify(x => x.Send(paymentEvent, It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.Is<GrowthAndSkillsPaymentsRecalculatedEvent>(e => e.Command == paymentEvent), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenLearningNotFound_ThrowsException()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Apprenticeship learning not found for key: {command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey}");
    }

    [Test]
    public async Task WhenEpisodeNotFound_ThrowsException()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        var (learning, _) = BuildLearning(apprenticeshipPayableEarningsUpdatedEvent.LearningKey, Guid.NewGuid());

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Apprenticeship episode not found for EpisodeKey: {command.ApprenticeshipPayableEarningsUpdatedEvent.EpisodeKey} on LearningKey: {command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey}");
    }

    [Test]
    public async Task WhenEnglishAndMathsCoursesWithInstalmentsPresent_ThenAdditionalPaymentEventSentPerCourse()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        var englishAndMathsCourse = _fixture.Build<EnglishAndMathsEntity>()
            .With(x => x.Instalments, new List<EnglishAndMathsInstalmentEntity>
            {
                _fixture.Build<EnglishAndMathsInstalmentEntity>()
                    .With(x => x.Type, EnglishAndMathsInstalmentType.Regular.ToString())
                    .Create()
            })
            .With(x => x.PeriodsInLearning, new List<EnglishAndMathsPeriodInLearningEntity>())
            .Create();

        var (learning, _) = BuildLearning(
            apprenticeshipPayableEarningsUpdatedEvent.LearningKey,
            apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey,
            new List<EnglishAndMathsEntity> { englishAndMathsCourse });
        var episode = learning.GetEpisode(apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        var onProgrammePaymentEvent = new CalculateGrowthAndSkillsPayments();
        var englishAndMathsPaymentEvent = new CalculateGrowthAndSkillsPayments();

        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(onProgrammePaymentEvent);
        _mockBuilder.Setup(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(englishAndMathsPaymentEvent);

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        _mockMessageSession.Verify(x => x.Send(onProgrammePaymentEvent, It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Send(englishAndMathsPaymentEvent, It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.Is<GrowthAndSkillsPaymentsRecalculatedEvent>(e => e.Command == onProgrammePaymentEvent), It.IsAny<PublishOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.Is<GrowthAndSkillsPaymentsRecalculatedEvent>(e => e.Command == englishAndMathsPaymentEvent), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenNoEnglishAndMathsCoursesWithInstalments_ThenOnlyOnProgrammeSendHappens()
    {
        var apprenticeshipPayableEarningsUpdatedEvent = _fixture.Create<ApprenticeshipPayableEarningsUpdatedEvent>();
        var command = new global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand.SendApprenticeshipPayableEarningsToPaymentsCommand(apprenticeshipPayableEarningsUpdatedEvent);

        // A course exists but has no instalments - should be excluded from the E&M push
        var englishAndMathsCourseWithNoInstalments = _fixture.Build<EnglishAndMathsEntity>()
            .With(x => x.Instalments, new List<EnglishAndMathsInstalmentEntity>())
            .With(x => x.PeriodsInLearning, new List<EnglishAndMathsPeriodInLearningEntity>())
            .Create();

        var (learning, _) = BuildLearning(
            apprenticeshipPayableEarningsUpdatedEvent.LearningKey,
            apprenticeshipPayableEarningsUpdatedEvent.EpisodeKey,
            new List<EnglishAndMathsEntity> { englishAndMathsCourseWithNoInstalments });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey))
            .ReturnsAsync(learning);

        var onProgrammePaymentEvent = new CalculateGrowthAndSkillsPayments();
        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(onProgrammePaymentEvent);

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), It.IsAny<ApprenticeshipLearning>(), It.IsAny<Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<GrowthAndSkillsPaymentsRecalculatedEvent>(), It.IsAny<PublishOptions>()), Times.Once);
    }
}
