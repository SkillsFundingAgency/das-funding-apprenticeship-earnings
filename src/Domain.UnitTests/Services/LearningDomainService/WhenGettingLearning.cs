using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Services.LearningDomainService;

[TestFixture]
public class WhenGettingLearning
{
    private Mock<ILearningRepository> _mockRepository = null!;
    private Domain.Services.LearningDomainService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<ILearningRepository>();
        _sut = new Domain.Services.LearningDomainService(_mockRepository.Object);
    }

    [Test]
    public async Task ThenApprenticeshipLearningIsReturnedWhenFound()
    {
        var key = Guid.NewGuid();
        var learning = BuildApprenticeshipLearning(key);
        _mockRepository.Setup(x => x.GetApprenticeshipLearning(key)).ReturnsAsync(learning);

        var result = await _sut.GetLearning(key);

        result.Should().BeSameAs(learning);
        _mockRepository.Verify(x => x.GetShortCourseLearning(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task ThenShortCourseLearningIsReturnedWhenApprenticeshipNotFound()
    {
        var key = Guid.NewGuid();
        var learning = BuildShortCourseLearning(key);
        _mockRepository.Setup(x => x.GetApprenticeshipLearning(key)).ReturnsAsync((ApprenticeshipLearning?)null);
        _mockRepository.Setup(x => x.GetShortCourseLearning(key)).ReturnsAsync(learning);

        var result = await _sut.GetLearning(key);

        result.Should().BeSameAs(learning);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNeitherFound()
    {
        var key = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetApprenticeshipLearning(key)).ReturnsAsync((ApprenticeshipLearning?)null);
        _mockRepository.Setup(x => x.GetShortCourseLearning(key)).ReturnsAsync((ShortCourseLearning?)null);

        var result = await _sut.GetLearning(key);

        result.Should().BeNull();
    }

    private static ApprenticeshipLearning BuildApprenticeshipLearning(Guid key)
    {
        var entity = new ApprenticeshipLearningEntity
        {
            LearningKey = key,
            Episodes = new List<ApprenticeshipEpisodeEntity>
            {
                new ApprenticeshipEpisodeEntity
                {
                    Prices = new List<ApprenticeshipEpisodePriceEntity>
                    {
                        new ApprenticeshipEpisodePriceEntity { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) }
                    },
                    PeriodsInLearning = new List<ApprenticeshipPeriodInLearningEntity>()
                }
            }
        };
        return ApprenticeshipLearning.Get(entity);
    }

    private static ShortCourseLearning BuildShortCourseLearning(Guid key)
    {
        var entity = new ShortCourseLearningEntity
        {
            LearningKey = key,
            Episodes = new List<ShortCourseEpisodeEntity> { new ShortCourseEpisodeEntity() }
        };
        return ShortCourseLearning.Get(entity);
    }
}
