using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests;

internal static class TestHelper
{
    internal static Apprenticeship BuildApprenticeship(this Fixture fixture)
    {
        var apprenticeshipEntityModel = fixture.BuildApprenticeshipModel();
        return Apprenticeship.Get(apprenticeshipEntityModel);
    }

    internal static ApprenticeshipModel BuildApprenticeshipModel(this Fixture fixture)
    {
        var episodeModel = fixture
            .Build<EpisodeModel>()
            .With(x => x.Prices, new List<EpisodePriceModel>{ fixture.Build<EpisodePriceModel>()
                .With(x => x.StartDate, DateTime.UtcNow.AddMonths(-10))
                .With(x => x.EndDate, DateTime.UtcNow.AddMonths(10))
                .Create() })
            .With(x => x.EarningsProfile, fixture
                .Build<EarningsProfileModel>()
                .With(x => x.Instalments, new List<InstalmentModel>())
                .With(x => x.AdditionalPayments, new List<AdditionalPaymentModel>())
                .With(x => x.MathsAndEnglishCourses, new List<MathsAndEnglishModel>())
                .Create())
            .Create();
        var apprenticeshipEntityModel = fixture
            .Build<ApprenticeshipModel>()
            .With(x => x.Episodes, new List<EpisodeModel> { episodeModel })
            .Create();

        return apprenticeshipEntityModel;
    }
}
