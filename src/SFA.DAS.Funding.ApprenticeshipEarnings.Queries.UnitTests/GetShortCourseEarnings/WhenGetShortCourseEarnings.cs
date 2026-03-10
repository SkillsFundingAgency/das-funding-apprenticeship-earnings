using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetShortCourseEarnings;

[TestFixture]
public class WhenGetShortCourseEarnings
{
    private Fixture _fixture;
    private Mock<ILearningRepository> _mockLearningRepository;
    private GetShortCourseEarningsQueryHandler _queryHandler;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockLearningRepository = new Mock<ILearningRepository>();
        _queryHandler = new GetShortCourseEarningsQueryHandler(
            _mockLearningRepository.Object,
            Mock.Of<ILogger<GetShortCourseEarningsQueryHandler>>());
    }

    [Test]
    public async Task Handle_LearningNotFound_ReturnsEmptyEarnings()
    {
        // Arrange
        var query = new GetShortCourseEarningsRequest(_fixture.Create<Guid>(), _fixture.Create<long>());
        _mockLearningRepository.Setup(x => x.GetShortCourseLearning(query.LearningKey))
            .ReturnsAsync((ShortCourseLearning?)null);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        result.Earnings.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_LearningExists_ReturnsMappedEarnings()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();
        var query = new GetShortCourseEarningsRequest(learningKey, ukprn);

        var instalments = new List<ShortCourseInstalmentEntity>
        {
            new() { Key = Guid.NewGuid(), AcademicYear = 2021, DeliveryPeriod = 7, Amount = 600, Type = "ThirtyPercentLearningComplete" },
            new() { Key = Guid.NewGuid(), AcademicYear = 2021, DeliveryPeriod = 11, Amount = 1400, Type = "LearningComplete" }
        };

        var learning = CreateShortCourseLearning(learningKey, ukprn, instalments);

        _mockLearningRepository.Setup(x => x.GetShortCourseLearning(learningKey))
            .ReturnsAsync(learning);

        // Act
        var result = await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
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
    public async Task Handle_UkprnDoesNotMatchEpisode_Throws()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();
        var query = new GetShortCourseEarningsRequest(learningKey, _fixture.Create<long>());
        var differentUkprn = _fixture.Create<long>();

        var learning = CreateShortCourseLearning(learningKey, differentUkprn, new List<ShortCourseInstalmentEntity>());

        _mockLearningRepository.Setup(x => x.GetShortCourseLearning(learningKey))
            .ReturnsAsync(learning);

        // Act
        var act = async () => await _queryHandler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ShortCourseLearning CreateShortCourseLearning(Guid learningKey, long ukprn, List<ShortCourseInstalmentEntity> instalments)
    {
        var earningsProfile = new ShortCourseEarningsProfileEntity
        {
            EarningsProfileId = Guid.NewGuid(),
            Instalments = instalments
        };

        var episode = new ShortCourseEpisodeEntity
        {
            Key = Guid.NewGuid(),
            LearningKey = learningKey,
            Ukprn = ukprn,
            StartDate = new DateTime(2021, 1, 1),
            EndDate = new DateTime(2021, 6, 25),
            EarningsProfile = earningsProfile
        };

        var entity = new ShortCourseLearningEntity
        {
            LearningKey = learningKey,
            DateOfBirth = new DateTime(1990, 1, 1),
            Episodes = new List<ShortCourseEpisodeEntity> { episode }
        };

        return ShortCourseLearning.Get(entity);
    }
}
