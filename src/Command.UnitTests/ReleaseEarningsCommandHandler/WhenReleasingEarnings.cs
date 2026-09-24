using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ReleaseEarningsCommandTests;

[TestFixture]
public class WhenReleasingEarnings
{
    private Fixture _fixture = null!;
    private Mock<ILearningRepository> _mockRepository = null!;
    private Mock<IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder> _mockBuilder = null!;
    private Mock<IMessageSession> _mockMessageSession = null!;
    private PaymentsConfiguration _paymentsConfiguration = null!;
    private ReleaseEarningsCommand.ReleaseEarningsCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<ILearningRepository>();
        _mockBuilder = new Mock<IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder>();
        _mockMessageSession = new Mock<IMessageSession>();
        _paymentsConfiguration = new PaymentsConfiguration { PaymentsEndpoint = "payments-queue-name" };

        _sut = new ReleaseEarningsCommand.ReleaseEarningsCommandHandler(
            _mockRepository.Object,
            _mockBuilder.Object,
            _mockMessageSession.Object,
            _paymentsConfiguration,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ReleaseEarningsCommand.ReleaseEarningsCommandHandler>>());
    }

    private ApprenticeshipEpisodeEntity BuildEpisodeEntity(
        Guid episodeKey,
        Guid learningKey,
        bool isApproved = true,
        bool isRemoved = false,
        FundingPlatform fundingPlatform = FundingPlatform.DAS,
        long? fundingEmployerAccountId = null,
        List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity>? englishAndMathsCourses = null)
    {
        return _fixture.Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.Key, episodeKey)
            .With(x => x.LearningKey, learningKey)
            .With(x => x.IsRemoved, isRemoved)
            .With(x => x.FundingPlatform, fundingPlatform)
            .With(x => x.FundingEmployerAccountId, fundingEmployerAccountId)
            .With(x => x.EarningsProfile, _fixture.Build<ApprenticeshipEarningsProfileEntity>()
                .With(x => x.IsApproved, isApproved)
                .With(x => x.Instalments, new List<ApprenticeshipInstalmentEntity>())
                .With(x => x.EnglishAndMathsCourses, englishAndMathsCourses ?? new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity>())
                .Create())
            .Create();
    }

    private DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity BuildEnglishAndMathsEntity(bool withInstalments = true)
    {
        return _fixture.Build<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity>()
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.CompletionDate, (DateTime?)null)
            .With(x => x.PauseDate, (DateTime?)null)
            .With(x => x.Instalments, withInstalments
                ? new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsInstalmentEntity>
                {
                    new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 1, Amount = 40m, Type = "Regular" }
                }
                : new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsInstalmentEntity>())
            .With(x => x.PeriodsInLearning, new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsPeriodInLearningEntity>())
            .Create();
    }

    private ApprenticeshipLearning BuildLearning(Guid learningKey, params ApprenticeshipEpisodeEntity[] episodeEntities)
    {
        var learningEntity = _fixture.Build<ApprenticeshipLearningEntity>()
            .With(x => x.LearningKey, learningKey)
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ApprenticeshipEpisodeEntity>(episodeEntities))
            .Create();

        return ApprenticeshipLearning.Get(learningEntity);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task WhenLearnerRefIsNotSet_ThenNoLookupIsMadeAndNoPaymentEventIsSent(string? learnerRef)
    {
        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            _fixture.Create<Guid>(),
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = learnerRef! });

        await _sut.Handle(command, CancellationToken.None);

        _mockRepository.Verify(x => x.GetApprenticeshipLearning(It.IsAny<Guid>()), Times.Never);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Never);
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<GrowthAndSkillsPaymentsRecalculatedEvent>(), It.IsAny<PublishOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenLearningNotFound_ThrowsException()
    {
        var learningKey = _fixture.Create<Guid>();
        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync((ApprenticeshipLearning?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Apprenticeship learning not found for key: {learningKey}");
    }

    [Test]
    public async Task ThenThePaymentEventIsBuiltAndSentAndRecalculatedEventPublished()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, fundingEmployerAccountId: _fixture.Create<long>());
        var learning = BuildLearning(learningKey, episodeEntity);
        var episode = learning.GetEpisode(episodeEntity.Key);

        var request = new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() };
        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(learningKey, request);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        var paymentEvent = new CalculateGrowthAndSkillsPayments();
        _mockBuilder.Setup(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                episodeEntity.EmployerAccountId,
                episodeEntity.FundingEmployerAccountId!.Value,
                request.LearnerKey,
                request.LearnerRef))
            .Returns(paymentEvent);

        await _sut.Handle(command, CancellationToken.None);

        _mockRepository.Verify(x => x.GetApprenticeshipLearning(learningKey), Times.Once);
        _mockBuilder.Verify(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                episodeEntity.EmployerAccountId,
                episodeEntity.FundingEmployerAccountId!.Value,
                request.LearnerKey,
                request.LearnerRef), Times.Once);
        _mockMessageSession.Verify(x => x.Send(paymentEvent, It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.Is<GrowthAndSkillsPaymentsRecalculatedEvent>(e => e.Command == paymentEvent), It.IsAny<PublishOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenFundingEmployerAccountIdIsNotSet_ThenEmployerAccountIdIsUsedAsFallback()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, fundingEmployerAccountId: null);
        var learning = BuildLearning(learningKey, episodeEntity);
        var episode = learning.GetEpisode(episodeEntity.Key);

        var request = new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() };
        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(learningKey, request);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        _mockBuilder.Setup(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                episodeEntity.EmployerAccountId,
                episodeEntity.EmployerAccountId,
                request.LearnerKey,
                request.LearnerRef))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.Build(
                (ApprenticeshipEpisode)episode,
                learning,
                episodeEntity.EmployerAccountId,
                episodeEntity.EmployerAccountId,
                request.LearnerKey,
                request.LearnerRef), Times.Once);
    }

    [Test]
    public async Task WhenFundingPlatformIsNotDas_ThenNoPaymentEventIsSentOrPublished()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, fundingPlatform: FundingPlatform.SLD);
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), It.IsAny<ApprenticeshipLearning>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Never);
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<GrowthAndSkillsPaymentsRecalculatedEvent>(), It.IsAny<PublishOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenEpisodeIsRemoved_ThenNoPaymentEventIsSent()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, isRemoved: true);
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        await _sut.Handle(command, CancellationToken.None);

        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenEpisodeIsNotApproved_ThenNoPaymentEventIsSent()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, isApproved: false);
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        await _sut.Handle(command, CancellationToken.None);

        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenEpisodeHasEnglishAndMathsCourses_ThenOnProgrammeAndEachCourseAreSent()
    {
        var learningKey = _fixture.Create<Guid>();
        var courseOne = BuildEnglishAndMathsEntity();
        var courseTwo = BuildEnglishAndMathsEntity();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey,
            englishAndMathsCourses: new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity> { courseOne, courseTwo });
        var learning = BuildLearning(learningKey, episodeEntity);

        var request = new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() };
        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(learningKey, request);

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());
        _mockBuilder.Setup(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(2));
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Exactly(3));
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<GrowthAndSkillsPaymentsRecalculatedEvent>(), It.IsAny<PublishOptions>()), Times.Exactly(3));
    }

    [Test]
    public async Task WhenEpisodeHasNoEnglishAndMathsCourses_ThenOnlyOnProgrammeIsSent()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey);
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);
        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), It.IsAny<ApprenticeshipLearning>(), It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Once);
    }

    [Test]
    public async Task WhenEnglishAndMathsCourseHasNoInstalments_ThenItIsStillSent()
    {
        var learningKey = _fixture.Create<Guid>();
        var courseWithNoInstalments = BuildEnglishAndMathsEntity(withInstalments: false);
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey,
            englishAndMathsCourses: new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity> { courseWithNoInstalments });
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);
        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());
        _mockBuilder.Setup(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Exactly(2));
    }

    [Test]
    public async Task WhenFundingPlatformIsNotDas_ThenNoEnglishAndMathsEventIsSentEither()
    {
        var learningKey = _fixture.Create<Guid>();
        var episodeEntity = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, fundingPlatform: FundingPlatform.SLD,
            englishAndMathsCourses: new List<DataAccess.Entities.EnglishAndMaths.EnglishAndMathsEntity> { BuildEnglishAndMathsEntity() });
        var learning = BuildLearning(learningKey, episodeEntity);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.BuildForEnglishAndMaths(It.IsAny<ApprenticeshipEpisode>(), It.IsAny<ApprenticeshipLearning>(), It.IsAny<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Never);
    }

    [Test]
    public async Task WhenLearningHasMultipleEpisodes_ThenOnlyApprovedNonRemovedDasEpisodesArePaid()
    {
        var learningKey = _fixture.Create<Guid>();

        var payableEpisode = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey);
        var removedEpisode = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, isRemoved: true);
        var unapprovedEpisode = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, isApproved: false);
        var nonDasEpisode = BuildEpisodeEntity(_fixture.Create<Guid>(), learningKey, fundingPlatform: FundingPlatform.SLD);

        var learning = BuildLearning(learningKey, payableEpisode, removedEpisode, unapprovedEpisode, nonDasEpisode);

        var command = new ReleaseEarningsCommand.ReleaseEarningsCommand(
            learningKey,
            new ReleaseEarningsCommand.ReleaseEarningsRequest { LearnerKey = _fixture.Create<Guid>(), LearnerRef = _fixture.Create<string>() });

        _mockRepository.Setup(x => x.GetApprenticeshipLearning(learningKey)).ReturnsAsync(learning);

        _mockBuilder.Setup(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new CalculateGrowthAndSkillsPayments());

        await _sut.Handle(command, CancellationToken.None);

        _mockBuilder.Verify(x => x.Build(It.IsAny<ApprenticeshipEpisode>(), learning, It.IsAny<long>(), It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
        _mockMessageSession.Verify(x => x.Send(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<SendOptions>()), Times.Once);
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<GrowthAndSkillsPaymentsRecalculatedEvent>(), It.IsAny<PublishOptions>()), Times.Once);
    }
}
