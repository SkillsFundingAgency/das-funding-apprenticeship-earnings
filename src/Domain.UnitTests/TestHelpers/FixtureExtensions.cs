using AutoFixture;
using Microsoft.Extensions.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship.Apprenticeship CreateApprenticeship(this Fixture fixture, 
        DateTime startDate, DateTime endDate, decimal agreedPrice, FundingType? fundingType = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.FundingBandMaximum = agreedPrice + 1;

        apprenticeshipEntityModel.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            new() { 
                UKPRN = 10000001, 
                EmployerAccountId = 10000001, 
                ActualStartDate = startDate, 
                PlannedEndDate = endDate,
                AgreedPrice = agreedPrice,
                FundingType = fundingType == null ? fixture.Create<FundingType>() : fundingType.Value
            }
        };

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }


    internal static Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(this Fixture fixture, Apprenticeship.Apprenticeship apprenticeship, decimal? newPrice = null, DateTime? newStartDate = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.ApprenticeshipKey = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;
        apprenticeshipEntityModel.LegalEntityName = apprenticeship.LegalEntityName;
        apprenticeshipEntityModel.FundingEmployerAccountId = apprenticeship.FundingEmployerAccountId;
        apprenticeshipEntityModel.FundingBandMaximum = newPrice == null ? apprenticeship.ApprenticeshipEpisodes.Single().AgreedPrice + 1 : newPrice.Value + 1;
        apprenticeshipEntityModel.AgeAtStartOfApprenticeship = apprenticeship.AgeAtStartOfApprenticeship;

        apprenticeshipEntityModel.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeModel
        {
            UKPRN = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            ActualStartDate = newStartDate == null ? x.ActualStartDate : newStartDate.Value,
            PlannedEndDate = x.PlannedEndDate,
            AgreedPrice = x.AgreedPrice,
            TrainingCode = x.TrainingCode,
            FundingType = x.FundingType
        }).ToList();

        apprenticeshipEntityModel.EarningsProfile = MapEarningsProfileToModel(apprenticeship.EarningsProfile);

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }

    internal static EarningsProfileEntityModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
    {
        var instalments = earningsProfile.Instalments.Select(i => new InstalmentEntityModel
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount
        }).ToList();

        return new EarningsProfileEntityModel
        {
            AdjustedPrice = earningsProfile.OnProgramTotal,
            Instalments = instalments,
            CompletionPayment = earningsProfile.CompletionPayment,
            EarningsProfileId = earningsProfile.EarningsProfileId
        };
    }
}
