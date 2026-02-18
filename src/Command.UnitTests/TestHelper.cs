using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests;

internal static class TestHelper
{
    internal static Apprenticeship BuildApprenticeship(this Fixture fixture)
    {
        var apprenticeshipEntityModel = fixture.BuildLearningModel();
        return Apprenticeship.Get(apprenticeshipEntityModel);
    }

    internal static LearningModel BuildLearningModel(this Fixture fixture)
    {
        var priceStartDate = DateTime.UtcNow.AddMonths(-10);
        var priceEndDate = DateTime.UtcNow.AddMonths(10);

        var episodeModel = fixture
            .Build<EpisodeModel>()
            .With(x => x.PeriodsInLearning, new List<EpisodePeriodInLearningModel>
            {
                fixture.Build<EpisodePeriodInLearningModel>()
                    .With(x => x.StartDate, priceStartDate)
                    .With(x => x.EndDate, priceEndDate)
                    .With(x => x.OriginalExpectedEndDate, priceEndDate)
                    .Create()
            })
            .With(x=> x.FundingBandMaximum, int.MaxValue)
            .With(x => x.Prices, new List<EpisodePriceModel>{ fixture.Build<EpisodePriceModel>()
                .With(x => x.StartDate, priceStartDate)
                .With(x => x.EndDate, priceEndDate)
                .Create() })
            .With(x => x.EarningsProfile, fixture
                .Build<EarningsProfileModel>()
                .With(x => x.Instalments, new List<InstalmentModel>())
                .With(x => x.AdditionalPayments, new List<AdditionalPaymentModel>())
                .With(x => x.MathsAndEnglishCourses, new List<MathsAndEnglishModel>())
                .Create())
            .Create();
        var learningEntityModel = fixture
            .Build<LearningModel>()
            .With(x => x.Episodes, new List<EpisodeModel> { episodeModel })
            .Create();

        return learningEntityModel;
    }

    internal static List<MathsAndEnglish> BuildMathsAndEnglishCourses(this Fixture fixture)
    {
        var courses = new List<MathsAndEnglish>
        {
            fixture.BuildMathsAndEnglish()
        };

        return courses;
    }

    internal static MathsAndEnglish BuildMathsAndEnglish(this Fixture fixture)
    {
        var model = fixture.BuildMathsAndEnglishModel();
        return MathsAndEnglish.Get(model);
    }

    internal static MathsAndEnglishModel BuildMathsAndEnglishModel(this Fixture fixture)
    {
        return fixture.Build<MathsAndEnglishModel>()
            .Create();
    }
}
