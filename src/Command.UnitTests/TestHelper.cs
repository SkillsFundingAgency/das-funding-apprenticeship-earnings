using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests;

internal static class TestHelper
{
    internal static Domain.Models.Learning BuildLearning(this Fixture fixture)
    {
        var apprenticeshipEntityModel = fixture.BuildLearningModel();
        return Domain.Models.Learning.Get(apprenticeshipEntityModel);
    }

    internal static LearningEntity BuildLearningModel(this Fixture fixture)
    {
        var priceStartDate = DateTime.UtcNow.AddMonths(-10);
        var priceEndDate = DateTime.UtcNow.AddMonths(10);

        var episodeModel = fixture
            .Build<ApprenticeshipEpisodeEntity>()
            .With(x => x.PeriodsInLearning, new List<ApprenticeshipPeriodInLearningEntity>
            {
                fixture.Build<ApprenticeshipPeriodInLearningEntity>()
                    .With(x => x.StartDate, priceStartDate)
                    .With(x => x.EndDate, priceEndDate)
                    .With(x => x.OriginalExpectedEndDate, priceEndDate)
                    .Create()
            })
            .With(x=> x.FundingBandMaximum, int.MaxValue)
            .With(x => x.Prices, new List<ApprenticeshipEpisodePriceEntity>{ fixture.Build<ApprenticeshipEpisodePriceEntity>()
                .With(x => x.StartDate, priceStartDate)
                .With(x => x.EndDate, priceEndDate)
                .Create() })
            .With(x => x.EarningsProfile, fixture
                .Build<ApprenticeshipEarningsProfileEntity>()
                .With(x => x.Instalments, new List<ApprenticeshipInstalmentEntity>())
                .With(x => x.ApprenticeshipAdditionalPayments, new List<ApprenticeshipAdditionalPaymentEntity>())
                .With(x => x.EnglishAndMathsCourses, new List<EnglishAndMathsEntity>())
                .Create())
            .Create();
        var learningEntityModel = fixture
            .Build<LearningEntity>()
            .With(x => x.ApprenticeshipEpisodes, new List<ApprenticeshipEpisodeEntity> { episodeModel })
            .Create();

        return learningEntityModel;
    }

    internal static List<EnglishAndMaths> BuildMathsAndEnglishCourses(this Fixture fixture)
    {
        var courses = new List<EnglishAndMaths>
        {
            fixture.BuildMathsAndEnglish()
        };

        return courses;
    }

    internal static EnglishAndMaths BuildMathsAndEnglish(this Fixture fixture)
    {
        var model = fixture.BuildMathsAndEnglishModel();
        return EnglishAndMaths.Get(model);
    }

    internal static EnglishAndMathsEntity BuildMathsAndEnglishModel(this Fixture fixture)
    {
        return fixture.Build<EnglishAndMathsEntity>()
            .Create();
    }
}
