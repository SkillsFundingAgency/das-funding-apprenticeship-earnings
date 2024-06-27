using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static ApprenticeshipEntityModel CreateApprenticeshipEntityModel(this Fixture fixture)
    {
        var apprenticeship = fixture.Create<ApprenticeshipEntityModel>();
        apprenticeship.ActualStartDate = new DateTime(2019, 09, 01); // DO NOT APPROVE PR WITH THESE HERE
        apprenticeship.PlannedEndDate = new DateTime(2020, 1, 1); // DO NOT APPROVE PR WITH THESE HERE
        apprenticeship.AgeAtStartOfApprenticeship = 21;
        apprenticeship.FundingBandMaximum = 20000;
        apprenticeship.EarningsProfile.AdjustedPrice = 10000;
        apprenticeship.EarningsProfile.CompletionPayment = 4000;
        apprenticeship.EarningsProfile.Instalments = new List<InstalmentEntityModel>
        {
            new() { AcademicYear = 1920, DeliveryPeriod = 2, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 3, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 4, Amount = 2500},
            new() { AcademicYear = 1920, DeliveryPeriod = 5, Amount = 2500}
        };
        apprenticeship.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            new() { 
                UKPRN = 10000001, 
                EmployerAccountId = 10000001, 
                ActualStartDate = new DateTime(2019, 09, 01), 
                PlannedEndDate = new DateTime(2020, 1, 1),
                AgreedPrice = 10000
            }
        };

        return apprenticeship;
    }
}
