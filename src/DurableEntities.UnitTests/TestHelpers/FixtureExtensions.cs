using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship CreateApprenticeship(this Fixture fixture,
    DateTime startDate, DateTime endDate)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            new() {
                UKPRN = 10000001,
                EmployerAccountId = 10000001,
                FundingEmployerAccountId = null,
                Prices = new List<PriceModel>
                {
                    new()
                    {
                        ActualStartDate = startDate,
                        PlannedEndDate = endDate,
                        AgreedPrice = fixture.Create<decimal>()
                    }
                }
            }
        };

        return new Apprenticeship(apprenticeshipEntityModel);
    }
}
