using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class ApprenticeshipEarningsProfileExtensions
{
    internal static EarningsProfileUpdatedEvent CreatedEarningsProfileUpdatedEvent(this ApprenticeshipEarningsProfileEntity earningsProfile, bool initialGeneration = false)
    {
        var archiveEarningsProfileEvent = new EarningsProfileUpdatedEvent
        {
            EarningsProfileId = earningsProfile.EarningsProfileId,
            EpisodeKey = earningsProfile.EpisodeKey,
            Version = earningsProfile.Version,
            OnProgramTotal = earningsProfile.OnProgramTotal,
            CompletionPayment = earningsProfile.CompletionPayment,
            Instalments = earningsProfile.GetInstalments(),
            AdditionalPayments = earningsProfile.GetAdditionalPayments(),
            EnglishAndMaths = earningsProfile.GetMathsAndEnglish(),
            InitialGeneration = initialGeneration
        };

        return archiveEarningsProfileEvent;
    }

    private static List<Instalment> GetInstalments(this ApprenticeshipEarningsProfileEntity earningsProfile)
    {
        if (earningsProfile.Instalments == null || !earningsProfile.Instalments.Any())
        {
            return new List<Instalment>();
        }

        return earningsProfile.Instalments.Select(i => new Instalment
        {
            Key = i.Key,
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            EpisodePriceKey = i.EpisodePriceKey,
            Type = i.Type
        }).OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod).ToList();
    }

    private static List<AdditionalPayment> GetAdditionalPayments(this ApprenticeshipEarningsProfileEntity earningsProfile)
    {
        if (earningsProfile.ApprenticeshipAdditionalPayments == null || !earningsProfile.ApprenticeshipAdditionalPayments.Any())
        {
            return new List<AdditionalPayment>();
        }

        return earningsProfile.ApprenticeshipAdditionalPayments.Select(ap => new AdditionalPayment
        {
            Key = ap.Key,
            AcademicYear = ap.AcademicYear,
            DeliveryPeriod = ap.DeliveryPeriod,
            Amount = ap.Amount,
            AdditionalPaymentType = ap.AdditionalPaymentType,
            DueDate = ap.DueDate
        }).OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod)
            .ToList();
    }

    internal static EnglishAndMathsEarningsProfileUpdatedEvent CreateEnglishAndMathsEarningsProfileUpdatedEvent(this EnglishAndMathsEntity englishAndMaths, Guid earningsProfileId)
    {
        return new EnglishAndMathsEarningsProfileUpdatedEvent
        {
            EarningsProfileId = earningsProfileId,
            EnglishAndMathsKey = englishAndMaths.Key,
            Version = englishAndMaths.Version,
            EnglishAndMaths = englishAndMaths.ToEnglishAndMaths()
        };
    }

    internal static List<EnglishAndMaths> GetMathsAndEnglish(this ApprenticeshipEarningsProfileEntity earningsProfile)
    {
        if (earningsProfile.EnglishAndMathsCourses == null || !earningsProfile.EnglishAndMathsCourses.Any())
        {
            return new List<EnglishAndMaths>();
        }

        return earningsProfile.EnglishAndMathsCourses.Select(ToEnglishAndMaths).ToList();
    }

    private static EnglishAndMaths ToEnglishAndMaths(this EnglishAndMathsEntity englishAndMaths)
    {
        return new EnglishAndMaths
        {
            EnglishAndMathsKey = englishAndMaths.Key,
            StartDate = englishAndMaths.StartDate,
            EndDate = englishAndMaths.EndDate,
            Course = englishAndMaths.Course,
            Amount = englishAndMaths.Amount,
            LearnAimRef = englishAndMaths.LearnAimRef,
            IsRemoved = englishAndMaths.IsRemoved,
            Instalments = englishAndMaths.Instalments.Select(i => new EnglishAndMathsInstalments
            {
                AcademicYear = i.AcademicYear,
                DeliveryPeriod = i.DeliveryPeriod,
                Amount = i.Amount
            }).OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod)
                .ToList()
        };
    }
}
