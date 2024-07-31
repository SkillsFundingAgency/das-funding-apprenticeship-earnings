using AutoFixture;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship.Apprenticeship CreateApprenticeship(this Fixture fixture, 
        DateTime startDate, DateTime endDate, decimal agreedPrice, FundingType? fundingType = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel>
        {
            new() { 
                ApprenticeshipEpisodeKey = Guid.NewGuid(),
                UKPRN = 10000001, 
                EmployerAccountId = 10000001, 
                FundingType = fundingType == null ? fixture.Create<FundingType>() : fundingType.Value,
                Prices = new List<PriceModel>{ new()
                {
                    PriceKey = Guid.NewGuid(),
                    ActualStartDate = startDate,
                    PlannedEndDate = endDate,
                    AgreedPrice = agreedPrice,
                    FundingBandMaximum = agreedPrice + 1
                }}
            }
        };

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }


    internal static Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(this Fixture fixture, Apprenticeship.Apprenticeship apprenticeship, decimal? newPrice = null, DateTime? newStartDate = null, bool withMissingEarningsProfile = false)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.ApprenticeshipKey = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;

        apprenticeshipEntityModel.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeModel
        {
            UKPRN = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            AgeAtStartOfApprenticeship = x.AgeAtStartOfApprenticeship,
            TrainingCode = x.TrainingCode,
            FundingType = x.FundingType,
            LegalEntityName = x.LegalEntityName,
            EarningsProfile = withMissingEarningsProfile ? null : MapEarningsProfileToModel(x.EarningsProfile!),
            FundingEmployerAccountId = x.FundingEmployerAccountId,
            Prices = MapPricesToModel(x.Prices, newPrice == null ? apprenticeship.ApprenticeshipEpisodes.Single().Prices!.Single().AgreedPrice + 1 : newPrice.Value + 1, newStartDate),
            ApprenticeshipEpisodeKey = x.ApprenticeshipEpisodeKey
        }).ToList();

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

    internal static List<PriceModel>? MapPricesToModel(List<Price>? prices, decimal fundingBandMaximum, DateTime? newStartDate)
    {
        return prices?.Select(x => new PriceModel
        {
            PriceKey = x.PriceKey,
            FundingBandMaximum = fundingBandMaximum,
            ActualStartDate = newStartDate ?? x.ActualStartDate,
            AgreedPrice = x.AgreedPrice,
            PlannedEndDate = x.PlannedEndDate
        }).ToList();
    }
}
