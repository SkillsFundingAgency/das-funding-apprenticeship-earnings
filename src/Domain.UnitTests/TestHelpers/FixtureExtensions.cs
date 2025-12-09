using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using FundingType = SFA.DAS.Learning.Enums.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    internal static Apprenticeship.Apprenticeship CreateApprenticeship(this Fixture fixture, 
        DateTime startDate, DateTime endDate, decimal agreedPrice, FundingType? fundingType = null, byte age = 17)
    {
        var apprenticeshipEntityModel = fixture
            .Build<LearningCreatedEvent>()
            .With(x => x.DateOfBirth, startDate.AddYears(-age))
            .Create();

        apprenticeshipEntityModel.Episode = new LearningEpisode()
        {
            Key = Guid.NewGuid(),
            Ukprn = 10000001, 
            EmployerAccountId = 10000001, 
            FundingType = fundingType == null ? fixture.Create<FundingType>() : fundingType.Value,
            FundingBandMaximum = int.MaxValue,
            Prices = new List<LearningEpisodePrice>{ new()
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = agreedPrice
                }
            },
            AgeAtStartOfLearning = age
        };

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }


    internal static Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(
        this Fixture fixture, 
        Apprenticeship.Apprenticeship apprenticeship, 
        decimal? newPrice = null, 
        DateTime? newStartDate = null, 
        bool withMissingEarningsProfile = false)
    {
        var apprenticeshipEntityModel = fixture.Create<ApprenticeshipModel>();

        apprenticeshipEntityModel.Key = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;

        apprenticeshipEntityModel.Episodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new EpisodeModel
        {
            Ukprn = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            AgeAtStartOfApprenticeship = x.AgeAtStartOfApprenticeship,
            TrainingCode = x.TrainingCode,
            FundingType = x.FundingType,
            LegalEntityName = x.LegalEntityName,
            EarningsProfile = withMissingEarningsProfile ? null : MapEarningsProfileToModel(x.EarningsProfile),
            FundingEmployerAccountId = x.FundingEmployerAccountId,
            FundingBandMaximum = x.FundingBandMaximum,
            Prices = MapPricesToModel(x.Prices, newStartDate),
            Key = x.ApprenticeshipEpisodeKey
        }).ToList();

        return Apprenticeship.Apprenticeship.Get(apprenticeshipEntityModel);
    }

    internal static EarningsProfileModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
    {
        var instalments = earningsProfile.Instalments.Select(i => new InstalmentModel
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            EpisodePriceKey = i.EpisodePriceKey
        }).ToList();

        var additionalPayments = earningsProfile.AdditionalPayments.Select(p => new AdditionalPaymentModel
            {
                AcademicYear = p.AcademicYear,
                DeliveryPeriod = p.DeliveryPeriod,
                Amount = p.Amount,
                DueDate = p.DueDate,
                AdditionalPaymentType = p.AdditionalPaymentType
            }).ToList();

        var mathAndEnglishCourses = earningsProfile.MathsAndEnglishCourses.Select(c => new MathsAndEnglishModel
        {
            Course = c.Course,
            WithdrawalDate = c.WithdrawalDate,
            StartDate = c.StartDate,
            Instalments = c.Instalments.Select(i => new MathsAndEnglishInstalmentModel
            {
                AcademicYear = i.AcademicYear,
                IsAfterLearningEnded = i.IsAfterLearningEnded,
                DeliveryPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                Type = i.Type.ToString()
            }).ToList(),
            Amount = c.Amount,
            ActualEndDate = c.ActualEndDate,
            EndDate = c.EndDate,
            PauseDate = c.PauseDate,
            PriorLearningAdjustmentPercentage = c.PriorLearningAdjustmentPercentage
        }).ToList();

        return new EarningsProfileModel
        {
            OnProgramTotal = earningsProfile.OnProgramTotal,
            Instalments = instalments,
            AdditionalPayments = additionalPayments,
            CompletionPayment = earningsProfile.CompletionPayment,
            EarningsProfileId = earningsProfile.EarningsProfileId,
            MathsAndEnglishCourses = mathAndEnglishCourses
        };
    }

    internal static List<EpisodePriceModel>? MapPricesToModel(IReadOnlyCollection<Price>? prices, DateTime? newStartDate)
    {
        return prices?.Select(x => new EpisodePriceModel
        {
            Key = x.PriceKey,
            StartDate = newStartDate ?? x.StartDate,
            AgreedPrice = x.AgreedPrice,
            EndDate = x.EndDate
        }).ToList();
    }
}
