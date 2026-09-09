using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using System;
using System.Collections.Generic;
using System.Linq;
using EmployerType = SFA.DAS.Funding.ApprenticeshipEarnings.Types.EmployerType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder;

[TestFixture]
public class WhenBuildingApprenticeshipCalculateGrowthAndSkillsPaymentsEvent
{
    private Fixture _fixture = null!;
    private Command.ApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _sut = new Command.ApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder();
    }

    private (ApprenticeshipLearning learning, ApprenticeshipEpisode episode, Guid priceKey) BuildLearning(
        DateTime startDate,
        DateTime endDate,
        decimal agreedPrice,
        List<ApprenticeshipInstalmentEntity>? instalments = null,
        DateTime? withdrawalDate = null,
        DateTime? completionDate = null,
        long employerAccountId = 0,
        long fundingEmployerAccountId = 0,
        EmployerType employerType = EmployerType.NonLevy,
        List<ApprenticeshipEpisodePriceEntity>? additionalPrices = null,
        Guid? priceKey = null)
    {
        var resolvedPriceKey = priceKey ?? Guid.NewGuid();
        var price = new ApprenticeshipEpisodePriceEntity
        {
            Key = resolvedPriceKey,
            StartDate = startDate,
            EndDate = endDate,
            AgreedPrice = agreedPrice
        };

        var prices = new List<ApprenticeshipEpisodePriceEntity> { price };
        if (additionalPrices != null)
            prices.AddRange(additionalPrices);

        var episodeEntity = _fixture.Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.WithdrawalDate, withdrawalDate)
            .With(x => x.CompletionDate, completionDate)
            .With(x => x.AchievementDate, (DateTime?)null)
            .With(x => x.PauseDate, (DateTime?)null)
            .With(x => x.EmployerAccountId, employerAccountId)
            .With(x => x.FundingEmployerAccountId, fundingEmployerAccountId)
            .With(x => x.EmployerType, employerType)
            .With(x => x.TrainingCode, "123-1")
            .With(x => x.Prices, prices)
            .With(x => x.EarningsProfile, _fixture.Build<ApprenticeshipEarningsProfileEntity>()
                .With(p => p.Instalments, instalments ?? new List<ApprenticeshipInstalmentEntity>())
                .With(p => p.EnglishAndMathsCourses, new List<EnglishAndMathsEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ApprenticeshipLearningEntity>()
            .With(x => x.Uln, "1234567890")
            .With(x => x.Episodes, new List<ApprenticeshipEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ApprenticeshipLearning.Get(learningEntity);
        var episode = (ApprenticeshipEpisode)learning.GetEpisode(episodeEntity.Key);

        return (learning, episode, resolvedPriceKey);
    }

    [Test]
    public void Then_BasicInformationCorrectlySet()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var employerAccountId = _fixture.Create<long>();
        var fundingAccountId = _fixture.Create<long>();
        var learnerKey = _fixture.Create<Guid>();
        var learnerReference = _fixture.Create<string>();

        var (learning, episode, _) = BuildLearning(startDate, endDate, 15000m);

        var result = _sut.Build(episode, learning, employerAccountId, fundingAccountId, learnerKey, learnerReference);

        result.EarningsId.Should().Be(episode.EarningsProfile!.Version);
        result.UKPRN.Should().Be(episode.UKPRN);
        result.EmployerContribution.Should().Be(0);

        result.Learner.Should().NotBeNull();
        result.Learner.LearnerKey.Should().Be(learnerKey);
        result.Learner.ULN.Should().Be(1234567890);
        result.Learner.Reference.Should().Be(learnerReference);

        result.Training.Should().NotBeNull();
        result.Training.LearningKey.Should().Be(learning.LearningKey);
        result.Training.CourseType.Should().Be(CourseType.Apprenticeship);
        result.Training.LearningType.Should().Be(LearningType.Apprenticeship);
        result.Training.CourseCode.Should().Be("123-1");
        result.Training.CourseReference.Should().Be("ZPROG0001");
        result.Training.AgeAtStartOfTraining.Should().Be((byte)episode.AgeAtStartOfApprenticeship);
        result.Training.StartDate.Should().Be(startDate);
        result.Training.PlannedEndDate.Should().Be(endDate);
    }

    [TestCase(false, false, TrainingStatus.Continuing)]
    [TestCase(false, true, TrainingStatus.Completed)]
    [TestCase(true, false, TrainingStatus.Withdrawn)]
    public void ThenTrainingStatusIsMappedCorrectly(bool withdrawn, bool completed, TrainingStatus expected)
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);

        var (learning, episode, _) = BuildLearning(
            startDate, endDate, 15000m,
            withdrawalDate: withdrawn ? new DateTime(2024, 1, 1) : null,
            completionDate: completed ? new DateTime(2024, 5, 1) : null);

        var result = _sut.Build(episode, learning, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>());

        result.Training.TrainingStatus.Should().Be(expected);
    }

    [Test]
    public void ThenAC1_InstalmentTypesMapToCorrectEarningTypes()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);

        var (learning, episode, priceKey) = BuildLearning(startDate, endDate, 15000m);
        var priceEntity = episode.Prices.Single();

        var instalments = new List<ApprenticeshipInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 1, Amount = 500m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Regular.ToString(), EpisodePriceKey = priceEntity.PriceKey },
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 12, Amount = 1000m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Completion.ToString(), EpisodePriceKey = priceEntity.PriceKey },
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 12, Amount = 250m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Balancing.ToString(), EpisodePriceKey = priceEntity.PriceKey }
        };

        (learning, episode, _) = BuildLearning(startDate, endDate, 15000m, instalments, priceKey: priceKey);

        var employerAccountId = _fixture.Create<long>();
        var fundingAccountId = _fixture.Create<long>();

        var result = _sut.Build(episode, learning, employerAccountId, fundingAccountId, _fixture.Create<Guid>(), _fixture.Create<string>());

        result.Earnings.Should().HaveCount(1);
        var earning = result.Earnings.Single();
        earning.AcademicYear.Should().Be(2324);
        earning.PricePeriods.Should().HaveCount(1);

        var periods = earning.PricePeriods.Single().Periods;
        periods.Should().HaveCount(3);

        var learningPeriod = periods.Single(p => p.EarningType == EarningType.Learning);
        learningPeriod.Amount.Should().Be(500m);
        learningPeriod.DeliveryPeriod.Should().Be(1);
        learningPeriod.LearningId.Should().Be(learning.ApprovalsApprenticeshipId);
        learningPeriod.Employer.AccountId.Should().Be(employerAccountId);
        learningPeriod.Employer.FundingAccountId.Should().Be(fundingAccountId);

        var completionPeriod = periods.Single(p => p.EarningType == EarningType.Completion);
        completionPeriod.Amount.Should().Be(1000m);
        completionPeriod.DeliveryPeriod.Should().Be(12);

        var balancingPeriod = periods.Single(p => p.EarningType == EarningType.Balancing);
        balancingPeriod.Amount.Should().Be(250m);
        balancingPeriod.DeliveryPeriod.Should().Be(12);
    }

    [Test]
    public void ThenAC3_WhenFundingAccountIdDiffersFromEmployerAccountId_LevyTransferInformationReachesPayload()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var employerAccountId = 111L;
        var fundingAccountId = 222L; // levy transfer sender differs from receiving employer

        var (learning, episode, priceKey) = BuildLearning(startDate, endDate, 15000m);
        var instalments = new List<ApprenticeshipInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 1, Amount = 500m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Regular.ToString(), EpisodePriceKey = priceKey }
        };
        (learning, episode, _) = BuildLearning(startDate, endDate, 15000m, instalments, employerAccountId: employerAccountId, fundingEmployerAccountId: fundingAccountId, priceKey: priceKey);

        var result = _sut.Build(episode, learning, employerAccountId, fundingAccountId, _fixture.Create<Guid>(), _fixture.Create<string>());

        var period = result.Earnings.Single().PricePeriods.Single().Periods.Single();
        period.Employer.AccountId.Should().Be(employerAccountId);
        period.Employer.FundingAccountId.Should().Be(fundingAccountId);
        period.Employer.AccountId.Should().NotBe(period.Employer.FundingAccountId);
    }

    [Test]
    public void ThenAC3_WhenNoLevyTransfer_EmployerAndFundingAccountIdsAreEqual()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var employerAccountId = 111L;

        var (learning, episode, priceKey) = BuildLearning(startDate, endDate, 15000m);
        var instalments = new List<ApprenticeshipInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 1, Amount = 500m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Regular.ToString(), EpisodePriceKey = priceKey }
        };
        (learning, episode, _) = BuildLearning(startDate, endDate, 15000m, instalments, employerAccountId: employerAccountId, fundingEmployerAccountId: employerAccountId, priceKey: priceKey);

        var result = _sut.Build(episode, learning, employerAccountId, employerAccountId, _fixture.Create<Guid>(), _fixture.Create<string>());

        var period = result.Earnings.Single().PricePeriods.Single().Periods.Single();
        period.Employer.AccountId.Should().Be(employerAccountId);
        period.Employer.FundingAccountId.Should().Be(employerAccountId);
    }

    [Test]
    public void WhenUnknownInstalmentType_ThenThrowsArgumentException()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);

        var (_, _, priceKey) = BuildLearning(startDate, endDate, 15000m);
        var instalments = new List<ApprenticeshipInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 1, Amount = 500m, Type = "SomeUnknownType", EpisodePriceKey = priceKey }
        };

        var (learning, episode, _) = BuildLearning(startDate, endDate, 15000m, instalments, priceKey: priceKey);

        var act = () => _sut.Build(episode, learning, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>());

        act.Should().Throw<Exception>();
    }

    [Test]
    public void WhenMultiplePricePeriodsAcrossAcademicYears_ThenMapsToMultipleEarningsGroupsWithCorrectPrices()
    {
        var firstPriceStart = new DateTime(2023, 9, 1);
        var firstPriceEnd = new DateTime(2024, 7, 31);
        var secondPriceStart = new DateTime(2024, 8, 1);
        var secondPriceEnd = new DateTime(2025, 6, 30);

        var secondPriceKey = Guid.NewGuid();
        var secondPrice = new ApprenticeshipEpisodePriceEntity
        {
            Key = secondPriceKey,
            StartDate = secondPriceStart,
            EndDate = secondPriceEnd,
            AgreedPrice = 9000m
        };

        var (learningInit, _, firstPriceKey) = BuildLearning(firstPriceStart, firstPriceEnd, 8000m, additionalPrices: new List<ApprenticeshipEpisodePriceEntity> { secondPrice });

        var instalments = new List<ApprenticeshipInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 11, Amount = 300m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Regular.ToString(), EpisodePriceKey = firstPriceKey },
            new() { Key = Guid.NewGuid(), AcademicYear = 2425, DeliveryPeriod = 2, Amount = 700m, Type = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.InstalmentType.Regular.ToString(), EpisodePriceKey = secondPriceKey }
        };

        var (learning, episode, _) = BuildLearning(firstPriceStart, firstPriceEnd, 8000m, instalments, additionalPrices: new List<ApprenticeshipEpisodePriceEntity> { secondPrice }, priceKey: firstPriceKey);

        var result = _sut.Build(episode, learning, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>());

        result.Earnings.Should().HaveCount(2);

        var firstYearEarning = result.Earnings.Single(e => e.AcademicYear == 2324);
        var firstPricePeriod = firstYearEarning.PricePeriods.Single();
        firstPricePeriod.Price.Should().Be(8000m);
        firstPricePeriod.StartDate.Should().Be(firstPriceStart);
        firstPricePeriod.EndDate.Should().Be(firstPriceEnd);

        var secondYearEarning = result.Earnings.Single(e => e.AcademicYear == 2425);
        var secondPricePeriod = secondYearEarning.PricePeriods.Single();
        secondPricePeriod.Price.Should().Be(9000m);
        secondPricePeriod.StartDate.Should().Be(secondPriceStart);
        secondPricePeriod.EndDate.Should().Be(secondPriceEnd);
    }

    [Test]
    public void BuildForEnglishAndMaths_ThenBasicInformationCorrectlySet()
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var (learning, episode, _) = BuildLearning(startDate, endDate, 15000m);

        var courseEntity = _fixture.Build<EnglishAndMathsEntity>()
            .With(x => x.LearnAimRef, "ENG12345")
            .With(x => x.StartDate, new DateTime(2023, 9, 1))
            .With(x => x.EndDate, new DateTime(2024, 7, 31))
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.CompletionDate, (DateTime?)null)
            .With(x => x.PauseDate, (DateTime?)null)
            .With(x => x.Instalments, new List<EnglishAndMathsInstalmentEntity>())
            .With(x => x.PeriodsInLearning, new List<EnglishAndMathsPeriodInLearningEntity>())
            .Create();

        var course = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths.Get(courseEntity);

        var result = _sut.BuildForEnglishAndMaths(episode, learning, course, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>());

        result.Training.LearningType.Should().Be(LearningType.MathsAndEnglish);
        result.Training.CourseCode.Should().Be("ENG12345");
        result.Training.CourseReference.Should().Be("ENG12345");
        result.Training.StartDate.Should().Be(courseEntity.StartDate);
        result.Training.PlannedEndDate.Should().Be(courseEntity.EndDate);
    }

    [TestCase("Regular", EarningType.OnProgrammeMathsAndEnglish)]
    [TestCase("Balancing", EarningType.BalancingMathsAndEnglish)]
    public void BuildForEnglishAndMaths_ThenInstalmentTypeMapsToCorrectEarningType(string instalmentType, EarningType expectedEarningType)
    {
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var (learning, episode, _) = BuildLearning(startDate, endDate, 15000m);

        var courseEntity = _fixture.Build<EnglishAndMathsEntity>()
            .With(x => x.LearnAimRef, "ENG12345")
            .With(x => x.Amount, 480m)
            .With(x => x.StartDate, new DateTime(2023, 9, 1))
            .With(x => x.EndDate, new DateTime(2024, 7, 31))
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.CompletionDate, (DateTime?)null)
            .With(x => x.PauseDate, (DateTime?)null)
            .With(x => x.Instalments, new List<EnglishAndMathsInstalmentEntity>
            {
                new() { Key = Guid.NewGuid(), AcademicYear = 2324, DeliveryPeriod = 3, Amount = 40m, Type = instalmentType }
            })
            .With(x => x.PeriodsInLearning, new List<EnglishAndMathsPeriodInLearningEntity>())
            .Create();

        var course = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths.Get(courseEntity);

        var result = _sut.BuildForEnglishAndMaths(episode, learning, course, _fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<Guid>(), _fixture.Create<string>());

        result.Earnings.Should().HaveCount(1);
        var period = result.Earnings.Single().PricePeriods.Single().Periods.Single();
        period.EarningType.Should().Be(expectedEarningType);
        period.Amount.Should().Be(40m);
        period.DeliveryPeriod.Should().Be(3);
    }
}
