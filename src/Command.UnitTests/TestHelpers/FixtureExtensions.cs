using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static ApprenticeshipEntityModel CreateApprenticeshipEntityModel(this Fixture fixture)
    {
        var apprenticeship = fixture.Create<ApprenticeshipEntityModel>();
        
        apprenticeship.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            new() { 
                UKPRN = 10000001, 
                EmployerAccountId = 10000001, 
                AgeAtStartOfApprenticeship = 21,
                Prices = new List<PriceModel>()
                {
                    new()
                    {
                        PriceKey = Guid.NewGuid(),
                        ActualStartDate = new DateTime(2019, 09, 01),
                        PlannedEndDate = new DateTime(2020, 1, 1),
                        AgreedPrice = 10000,
                        FundingBandMaximum = 20000
                    }
                },
                EarningsProfile = new EarningsProfileEntityModel
                {
                    EarningsProfileId = fixture.Create<Guid>(),
                    AdjustedPrice = 10000,
                    CompletionPayment = 4000,
                    Instalments = new List<InstalmentEntityModel>
                    {
                        new() { AcademicYear = 1920, DeliveryPeriod = 2, Amount = 2500},
                        new() { AcademicYear = 1920, DeliveryPeriod = 3, Amount = 2500},
                        new() { AcademicYear = 1920, DeliveryPeriod = 4, Amount = 2500},
                        new() { AcademicYear = 1920, DeliveryPeriod = 5, Amount = 2500}
                    }
                }
            }
        };

        return apprenticeship;
    }
}
