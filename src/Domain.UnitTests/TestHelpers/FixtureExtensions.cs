using AutoFixture;
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
    internal static Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(this Fixture fixture, Apprenticeship.Apprenticeship apprenticeship, decimal? newPrice = null, DateTime? newStartDate = null)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipEntityModel>();



        apprenticeshipEntityModel.ApprenticeshipKey = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;
        apprenticeshipEntityModel.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeModel { UKPRN = x.UKPRN }).ToList();
        apprenticeshipEntityModel.EmployerAccountId = apprenticeship.EmployerAccountId;
        apprenticeshipEntityModel.LegalEntityName = apprenticeship.LegalEntityName;
        apprenticeshipEntityModel.ActualStartDate = newStartDate == null ? apprenticeship.ActualStartDate : newStartDate.Value;
        apprenticeshipEntityModel.PlannedEndDate = apprenticeship.PlannedEndDate;
        apprenticeshipEntityModel.AgreedPrice = apprenticeship.AgreedPrice;
        apprenticeshipEntityModel.TrainingCode = apprenticeship.TrainingCode;
        apprenticeshipEntityModel.FundingEmployerAccountId = apprenticeship.FundingEmployerAccountId;
        apprenticeshipEntityModel.FundingType = apprenticeship.FundingType;
        apprenticeshipEntityModel.FundingBandMaximum = newPrice == null ? apprenticeship.AgreedPrice + 1 : newPrice.Value + 1;
        apprenticeshipEntityModel.AgeAtStartOfApprenticeship = apprenticeship.AgeAtStartOfApprenticeship;


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
