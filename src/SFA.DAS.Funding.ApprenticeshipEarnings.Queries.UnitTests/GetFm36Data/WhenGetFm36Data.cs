using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetFm36Data;

[TestFixture]
public class WhenGetFm36Data
{
    private ApprenticeshipEarningsDataContext _dbContext;
    private GetFm36DataQueryHandler _queryHandler;

    // Fixed "now" within academic year 2425 (AY start Aug 2024, end Jul 2025)
    private static readonly DateTime SearchDate = new DateTime(2025, 1, 1);
    private static readonly short CollectionYear = 2425;
    private static readonly byte CollectionPeriod = 6; // January in AY 2425

    // AY 2425 boundaries
    private static readonly DateTime AcademicYearStart = new DateTime(2024, 8, 1);
    private static readonly DateTime AcademicYearEnd = new DateTime(2025, 7, 31);

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApprenticeshipEarningsDataContext(options);
        _queryHandler = new GetFm36DataQueryHandler(
            _dbContext,
            Mock.Of<ILogger<GetFm36DataQueryHandler>>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task Handle_NoApprenticeships_ReturnsEmptyResponse()
    {
        var query = new GetFm36DataRequest(10005077, CollectionYear, CollectionPeriod, new List<Guid>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().BeNull();
    }

    [Test]
    public async Task Handle_ApprenticeshipsExist_ReturnsMappedResponse()
    {
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();
        var startDate = SearchDate.AddMonths(-6);
        var endDate = SearchDate.AddMonths(12);

        var instalment = new ApprenticeshipInstalmentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = CollectionYear,
            DeliveryPeriod = CollectionPeriod,
            Amount = 500m,
            Type = "Regular",
            EpisodePriceKey = Guid.NewGuid()
        };

        var additionalPayment = new ApprenticeshipAdditionalPaymentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = CollectionYear,
            DeliveryPeriod = CollectionPeriod,
            Amount = 150m,
            AdditionalPaymentType = "EmployerIncentive",
            DueDate = SearchDate
        };

        var englishAndMathsCourse = new EnglishAndMathsEntity
        {
            Key = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            Course = "English GCSE",
            LearnAimRef = "ENG_GCSE",
            Amount = 1900m,
            Instalments = startDate.Enumerate(endDate, DateIncrement.Monthly)
                .Select((date) => { 
                    (var academicYear, var deliveryPeriod) = date.ToAcademicYearAndPeriod();
                    return new EnglishAndMathsInstalmentEntity
                    {
                        Key = Guid.NewGuid(),
                        AcademicYear = academicYear,
                        DeliveryPeriod = deliveryPeriod,
                        Amount = 100m,
                        Type = "Regular"
                    };
                })
                .ToList()
        };

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: startDate,
            priceEndDate: endDate,
            instalment: instalment,
            additionalPayment: additionalPayment,
            englishAndMaths: englishAndMathsCourse,
            dateOfBirth: new DateTime(2000, 6, 1),
            onProgramTotal: 4500m,
            completionPayment: 500m);

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid> { learningKey });

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().HaveCount(1);

        var apprenticeship = result.Apprenticeships.Single();
        apprenticeship.Key.Should().Be(learningKey);
        apprenticeship.Ukprn.Should().Be(ukprn);
        apprenticeship.FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        apprenticeship.Episodes.Should().HaveCount(1);

        var episode = apprenticeship.Episodes.Single();
        episode.NumberOfInstalments.Should().Be(1);
        episode.CompletionPayment.Should().Be(500m);
        episode.OnProgramTotal.Should().Be(4500m);

        episode.Instalments.Should().ContainSingle(i =>
            i.AcademicYear == CollectionYear &&
            i.DeliveryPeriod == CollectionPeriod &&
            i.Amount == 500m &&
            i.InstalmentType == "Regular");

        episode.AdditionalPayments.Should().ContainSingle(p =>
            p.AcademicYear == CollectionYear &&
            p.DeliveryPeriod == CollectionPeriod &&
            p.Amount == 150m &&
            p.AdditionalPaymentType == "EmployerIncentive");

        episode.EnglishAndMaths.Should().ContainSingle(c =>
            c.Course == "English GCSE" &&
            c.LearnAimRef == "ENG_GCSE" &&
            c.StartDate == startDate &&
            c.EndDate == endDate &&
            c.Instalments.Where(i => i.Amount == 100m ).Count() == 19);

    }

    [Test]
    public async Task Handle_ApprenticeshipsExist_Under19_Returns1618FundingLineType()
    {
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: SearchDate.AddMonths(-6),
            priceEndDate: SearchDate.AddMonths(12),
            dateOfBirth: new DateTime(2007, 1, 1));

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid> { learningKey });

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Single().FundingLineType.Should().Be("16-18 Apprenticeship (Employer on App Service)");
    }

    [Test]
    public async Task Handle_UkprnDoesNotMatchCurrentEpisode_ReturnsEmptyResponse()
    {
        var learningKey = Guid.NewGuid();
        const long storedUkprn = 99999999;
        const long queryUkprn = 10005077;

        var learning = BuildLearning(learningKey, storedUkprn,
            priceStartDate: SearchDate.AddMonths(-6),
            priceEndDate: SearchDate.AddMonths(12));

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(queryUkprn, CollectionYear, CollectionPeriod, new List<Guid> { learningKey });

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().BeNull();
    }

    [Test]
    public async Task Handle_ContinuingApprenticeship_PlannedEndDateOneDayBeforeCurrentAYStart_IsIncluded()
    {
        // Learner started in a previous AY, planned end date is July 31 (last day before AY start),
        // but has no completion or withdrawal — they are still in learning.
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: new DateTime(2023, 8, 1),
            priceEndDate: AcademicYearStart.AddDays(-1)); // 2024-07-31 — one day before AY start

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().HaveCount(1);
        result.Apprenticeships.Single().Key.Should().Be(learningKey);
    }

    [Test]
    public async Task Handle_ContinuingApprenticeship_WithdrawalBeforeAYStart_IsExcluded()
    {
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: new DateTime(2023, 8, 1),
            priceEndDate: AcademicYearStart.AddDays(-1),
            withdrawalDate: AcademicYearStart.AddDays(-1)); // withdrawn before AY starts

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().BeNull();
    }

    [Test]
    public async Task Handle_ContinuingApprenticeship_CompletionBeforeAYStart_IsExcluded()
    {
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: new DateTime(2023, 8, 1),
            priceEndDate: AcademicYearStart.AddDays(-1),
            completionDate: AcademicYearStart.AddDays(-30)); // completed a month before AY starts

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().BeNull();
    }

    [Test]
    public async Task Handle_ContinuingApprenticeship_WithdrawalWithinAY_IsIncluded()
    {
        const long ukprn = 10005077;
        var learningKey = Guid.NewGuid();

        var learning = BuildLearning(learningKey, ukprn,
            priceStartDate: new DateTime(2023, 8, 1),
            priceEndDate: AcademicYearStart.AddDays(-1),
            withdrawalDate: AcademicYearStart.AddMonths(2)); // withdrew two months into current AY

        _dbContext.ApprenticeshipLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();

        var query = new GetFm36DataRequest(ukprn, CollectionYear, CollectionPeriod, new List<Guid>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Apprenticeships.Should().HaveCount(1);
        result.Apprenticeships.Single().Key.Should().Be(learningKey);
    }

    private static ApprenticeshipLearningEntity BuildLearning(
        Guid learningKey,
        long ukprn,
        DateTime priceStartDate,
        DateTime priceEndDate,
        ApprenticeshipInstalmentEntity? instalment = null,
        ApprenticeshipAdditionalPaymentEntity? additionalPayment = null,
        EnglishAndMathsEntity? englishAndMaths = null,
        DateTime? dateOfBirth = null,
        decimal onProgramTotal = 0m,
        decimal completionPayment = 0m,
        DateTime? withdrawalDate = null,
        DateTime? completionDate = null)
    {
        var episodeKey = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var priceKey = Guid.NewGuid();

        if (instalment != null) instalment.EarningsProfileId = profileId;
        if (additionalPayment != null) additionalPayment.EarningsProfileId = profileId;
        if (englishAndMaths != null) englishAndMaths.EarningsProfileId = profileId;


        var profile = new ApprenticeshipEarningsProfileEntity
        {
            EarningsProfileId = profileId,
            EpisodeKey = episodeKey,
            CalculationData = "{}",
            OnProgramTotal = onProgramTotal,
            CompletionPayment = completionPayment,
            Instalments = instalment != null ? [instalment] : [],
            ApprenticeshipAdditionalPayments = additionalPayment != null ? [additionalPayment] : [],
            EnglishAndMathsCourses = englishAndMaths != null ? [englishAndMaths] : []
        };

        var episode = new ApprenticeshipEpisodeEntity
        {
            Key = episodeKey,
            LearningKey = learningKey,
            Ukprn = ukprn,
            LegalEntityName = "Test Employer",
            TrainingCode = "ST0001",
            FundingType = FundingType.Levy,
            EarningsProfile = profile,
            WithdrawalDate = withdrawalDate,
            CompletionDate = completionDate,
            Prices =
            [
                new ApprenticeshipEpisodePriceEntity
                {
                    Key = priceKey,
                    EpisodeKey = episodeKey,
                    StartDate = priceStartDate,
                    EndDate = priceEndDate,
                    AgreedPrice = 5000m
                }
            ]
        };

        return new ApprenticeshipLearningEntity
        {
            LearningKey = learningKey,
            Uln = "1234567890",
            DateOfBirth = dateOfBirth ?? new DateTime(1990, 1, 1),
            Episodes = [episode]
        };
    }
}
