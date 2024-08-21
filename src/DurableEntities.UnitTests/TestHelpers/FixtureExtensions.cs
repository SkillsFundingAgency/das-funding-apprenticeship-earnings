using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship CreateApprenticeship(this Fixture fixture,
    DateTime startDate, DateTime endDate, Guid? apprenticeshipKey = null, Guid? episodeKey = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        if(apprenticeshipKey.HasValue)
        {
            apprenticeshipEntityModel.ApprenticeshipKey = apprenticeshipKey.Value;
        }

        apprenticeshipEntityModel.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            fixture.CreateApprenticeshipEpisodeModel(startDate, endDate, episodeKey)
        };

        return new Apprenticeship(apprenticeshipEntityModel);
    }

    internal static ApprenticeshipEpisodeModel CreateApprenticeshipEpisodeModel(this Fixture fixture, 
        DateTime startDate, DateTime endDate, Guid? episodeKey = null, long ukprn = 10000001, long employerAccountNumber = 10000001)
    {
        var apprenticeshipEpisodeModel = fixture.Create<ApprenticeshipEpisodeModel>();

        if(episodeKey.HasValue)
        {
            apprenticeshipEpisodeModel.ApprenticeshipEpisodeKey = episodeKey.Value;
        }

        apprenticeshipEpisodeModel.UKPRN = ukprn;
        apprenticeshipEpisodeModel.EmployerAccountId = employerAccountNumber;

        apprenticeshipEpisodeModel.Prices = new List<PriceModel>
        {
            new()
            {
                ActualStartDate = startDate,
                PlannedEndDate = endDate,
                AgreedPrice = fixture.Create<decimal>()
            }
        };

        return apprenticeshipEpisodeModel;
    }
}
