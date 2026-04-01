using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetShortCourseEarnings;

[TestFixture]
public class WhenGetShortCourseEarnings
{
    private ApprenticeshipEarningsDataContext _dbContext;
    private GetFm99ShortCourseEarningsQueryHandler _queryHandler;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApprenticeshipEarningsDataContext(options);
        _queryHandler = new GetFm99ShortCourseEarningsQueryHandler(
            _dbContext,
            Mock.Of<ILogger<GetFm99ShortCourseEarningsQueryHandler>>());
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    [Test]
    public async Task Handle_LearningNotFound_ReturnsEmptyEarnings()
    {
        var query = new GetFm99ShortCourseEarningsRequest(Guid.NewGuid(), 10005077);

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Earnings.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_LearningExists_ReturnsMappedEarnings()
    {
        var learningKey = Guid.NewGuid();
        const long ukprn = 10005077;
        var query = new GetFm99ShortCourseEarningsRequest(learningKey, ukprn);

        await SeedEpisodeWithInstalments(learningKey, ukprn, new List<ShortCourseInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2021, DeliveryPeriod = 7, Amount = 600, Type = "ThirtyPercentLearningComplete" },
            new() { Key = Guid.NewGuid(), AcademicYear = 2021, DeliveryPeriod = 11, Amount = 1400, Type = "LearningComplete" }
        });

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Earnings.Should().HaveCount(2);
        result.Earnings.Should().ContainSingle(e =>
            e.CollectionYear == 2021 &&
            e.CollectionPeriod == 7 &&
            e.Amount == 600 &&
            e.Type == "ThirtyPercentLearningComplete");
        result.Earnings.Should().ContainSingle(e =>
            e.CollectionYear == 2021 &&
            e.CollectionPeriod == 11 &&
            e.Amount == 1400 &&
            e.Type == "LearningComplete");
    }

    [Test]
    public async Task Handle_UkprnDoesNotMatchEpisode_ReturnsEmptyEarnings()
    {
        var learningKey = Guid.NewGuid();
        var query = new GetFm99ShortCourseEarningsRequest(learningKey, 10005077);

        await SeedEpisodeWithInstalments(learningKey, ukprn: 99999999, new List<ShortCourseInstalmentEntity>());

        var result = await _queryHandler.Handle(query, CancellationToken.None);

        result.Earnings.Should().BeEmpty();
    }

    private async Task SeedEpisodeWithInstalments(Guid learningKey, long ukprn, List<ShortCourseInstalmentEntity> instalments)
    {
        var earningsProfile = new ShortCourseEarningsProfileEntity
        {
            EarningsProfileId = Guid.NewGuid(),
            CalculationData = "{}",
            Instalments = instalments
        };

        var episode = new ShortCourseEpisodeEntity
        {
            Key = Guid.NewGuid(),
            LearningKey = learningKey,
            Ukprn = ukprn,
            LegalEntityName = "Test Employer",
            TrainingCode = "SC001",
            StartDate = new DateTime(2021, 1, 1),
            EndDate = new DateTime(2021, 6, 25),
            EarningsProfile = earningsProfile
        };

        var learning = new ShortCourseLearningEntity
        {
            LearningKey = learningKey,
            Uln = "1234567890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Episodes = new List<ShortCourseEpisodeEntity> { episode }
        };

        _dbContext.ShortCourseLearnings.Add(learning);
        await _dbContext.SaveChangesAsync();
    }
}
