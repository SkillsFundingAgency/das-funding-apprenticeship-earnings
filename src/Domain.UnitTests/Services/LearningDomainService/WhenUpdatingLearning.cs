using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
public class WhenUpdatingLearning
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
    public async Task ThenApprenticeshipUpdateIsCalledForApprenticeshipLearning()
    {
        var learning = BuildApprenticeshipLearning();

        await _sut.Update(learning);

        _mockRepository.Verify(x => x.Update(learning), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearning>()), Times.Never);
    }

    [Test]
    public async Task ThenShortCourseUpdateIsCalledForShortCourseLearning()
    {
        var learning = BuildShortCourseLearning();

        await _sut.Update(learning);

        _mockRepository.Verify(x => x.Update(learning), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }

    private static ApprenticeshipLearning BuildApprenticeshipLearning()
    {
        var entity = new ApprenticeshipLearningEntity
        {
            LearningKey = Guid.NewGuid(),
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

    private static ShortCourseLearning BuildShortCourseLearning()
    {
        var entity = new ShortCourseLearningEntity
        {
            LearningKey = Guid.NewGuid(),
            Episodes = new List<ShortCourseEpisodeEntity> { new ShortCourseEpisodeEntity() }
        };
        return ShortCourseLearning.Get(entity);
    }
}
