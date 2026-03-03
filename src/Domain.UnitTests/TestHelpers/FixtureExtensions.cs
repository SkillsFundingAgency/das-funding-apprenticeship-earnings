using AutoFixture;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using FundingType = SFA.DAS.Learning.Enums.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

internal static class FixtureExtensions
{
    private static ApprenticeshipFactory _apprenticeshipFactory = new ApprenticeshipFactory();

    internal static Models.Learning CreateLearning(
        this Fixture fixture,
        FundingType? fundingType = null,
        byte age = 17)
    {
        return fixture.CreateLearningWithApprenticeship(
            new DateTime(2021, 1, 15),
            new DateTime(2022, 1, 31), 
            7000,
            fundingType, 
            age);
    }

    internal static Models.Learning CreateLearningWithApprenticeship(
        this Fixture fixture, 
        DateTime startDate, 
        DateTime endDate, 
        decimal agreedPrice, 
        FundingType? fundingType = null, 
        byte age = 17)
    {
        var learningCreatedEvent = fixture.CreateLearningCreatedEvent(startDate, endDate, agreedPrice, age);

        learningCreatedEvent.Episode.FundingType = fundingType == null ? fixture.Create<FundingType>() : fundingType.Value;

        return _apprenticeshipFactory.CreateNew(learningCreatedEvent, int.MaxValue);
    }

    internal static Models.Learning CreateLearningWithShortCourse(
        this Fixture fixture,
        DateTime startDate,
        DateTime endDate,
        decimal agreedPrice)
    {
        var createRequest = fixture.Create<CreateUnapprovedShortCourseLearningRequest>();

        createRequest.OnProgramme.StartDate = startDate;
        createRequest.OnProgramme.ExpectedEndDate = endDate;
        createRequest.OnProgramme.TotalPrice = agreedPrice;

        return _apprenticeshipFactory.CreateNewShortCourse(createRequest);
    }


    internal static Models.Learning CreateUpdatedApprenticeship(
        this Fixture fixture, 
        Models.Learning apprenticeship, 
        decimal? newPrice = null, 
        DateTime? newStartDate = null, 
        bool withMissingEarningsProfile = false)
    {
        var apprenticeshipEntityModel = fixture.Create<LearningEntity>();

        apprenticeshipEntityModel.LearningKey = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;

        apprenticeshipEntityModel.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeEntity
        {
            Ukprn = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            TrainingCode = x.TrainingCode,
            FundingType = x.FundingType,
            LegalEntityName = x.LegalEntityName,
            EarningsProfile = withMissingEarningsProfile ? null : MapEarningsProfileToModel(x.EarningsProfile),
            FundingEmployerAccountId = x.FundingEmployerAccountId,
            FundingBandMaximum = x.FundingBandMaximum,
            Prices = MapPricesToModel(x.Prices, newStartDate),
            Key = x.EpisodeKey,
            PeriodsInLearning = x.EpisodePeriodsInLearning.Select(p => new ApprenticeshipPeriodInLearningEntity
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                OriginalExpectedEndDate = p.OriginalExpectedEndDate
            }).ToList()
        }).ToList();

        return Models.Learning.Get(apprenticeshipEntityModel);
    }

    internal static LearningCreatedEvent CreateLearningCreatedEvent(
        this Fixture fixture, 
        DateTime? startDate = null, 
        DateTime? endDate = null,
        decimal? agreedPrice = null,
        byte age = 17)
    {
        startDate ??= new DateTime(2021, 1, 15);
        endDate ??= new DateTime(2022, 1, 31);
        agreedPrice ??= 7000;

        var createdEvent = fixture
            .Build<LearningCreatedEvent>()
            .With(x => x.DateOfBirth, startDate.Value.AddYears(-age))
            .Create();

        createdEvent.Episode = new LearningEpisode()
        {
            Key = Guid.NewGuid(),
            Ukprn = 10000001,
            EmployerAccountId = 10000001,
            FundingBandMaximum = int.MaxValue,
            Prices = new List<LearningEpisodePrice>{ new()
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalPrice = agreedPrice.Value
                }
            },
            AgeAtStartOfLearning = age
        };

        return createdEvent;
    }

    internal static ApprenticeshipEarningsProfileEntity MapEarningsProfileToModel(ApprenticeshipEarningsProfile earningsProfile)
    {
        var instalments = earningsProfile.Instalments.Select(i => new ApprenticeshipInstalmentEntity
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            EpisodePriceKey = i.EpisodePriceKey
        }).ToList();

        var additionalPayments = earningsProfile.AdditionalPayments.Select(p => new ApprenticeshipAdditionalPaymentEntity
            {
                AcademicYear = p.AcademicYear,
                DeliveryPeriod = p.DeliveryPeriod,
                Amount = p.Amount,
                DueDate = p.DueDate,
                AdditionalPaymentType = p.AdditionalPaymentType
            }).ToList();

        var mathAndEnglishCourses = earningsProfile.MathsAndEnglishCourses.Select(c => new EnglishAndMathsEntity
        {
            Course = c.Course,
            WithdrawalDate = c.WithdrawalDate,
            StartDate = c.StartDate,
            Instalments = c.Instalments.Select(i => new EnglishAndMathsInstalmentEntity
            {
                AcademicYear = i.AcademicYear,
                DeliveryPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                Type = i.Type.ToString()
            }).ToList(),
            Amount = c.Amount,
            CompletionDate = c.CompletionDate,
            EndDate = c.EndDate,
            PauseDate = c.PauseDate,
            PriorLearningAdjustmentPercentage = c.PriorLearningAdjustmentPercentage
        }).ToList();

        return new ApprenticeshipEarningsProfileEntity
        {
            OnProgramTotal = earningsProfile.OnProgramTotal,
            Instalments = instalments,
            ApprenticeshipAdditionalPayments = additionalPayments,
            CompletionPayment = earningsProfile.CompletionPayment,
            EarningsProfileId = earningsProfile.EarningsProfileId,
            EnglishAndMathsCourses = mathAndEnglishCourses
        };
    }

    internal static List<ApprenticeshipEpisodePriceEntity>? MapPricesToModel(IReadOnlyCollection<ApprenticeshipPrice>? prices, DateTime? newStartDate)
    {
        return prices?.Select(x => new ApprenticeshipEpisodePriceEntity
        {
            Key = x.PriceKey,
            StartDate = newStartDate ?? x.StartDate,
            AgreedPrice = x.AgreedPrice,
            EndDate = x.EndDate
        }).ToList();
    }

    private static void SetDateOfBirth(LearningEntity apprenticeshipEntityModel, int age)
    {
        var startDate = apprenticeshipEntityModel.DateOfBirth = apprenticeshipEntityModel
            .ApprenticeshipEpisodes.SelectMany(x => x.Prices)
            .Select(x => x.StartDate)
            .Min();

        var dateOfBirth = startDate.AddYears(-age).AddDays(1);
        apprenticeshipEntityModel.DateOfBirth = dateOfBirth;

    }
}
