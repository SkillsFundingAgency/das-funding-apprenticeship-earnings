using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

internal static class EarningsProfileExtensions
{
    internal static EarningsProfileUpdatedEvent CreatedEarningsProfileUpdatedEvent(this EarningsProfileModel earningsProfile)
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
            EnglishAndMaths = earningsProfile.GetMathsAndEnglish()
        };

        return archiveEarningsProfileEvent;
    }

    private static List<Types.Instalment> GetInstalments(this EarningsProfileModel earningsProfile)
    {
        if (earningsProfile.Instalments == null || !earningsProfile.Instalments.Any())
        {
            return new List<Types.Instalment>();
        }

        return earningsProfile.Instalments.Select(i => new Types.Instalment
        {
            Key = i.Key,
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            EpisodePriceKey = i.EpisodePriceKey,
            Type = i.Type.Trim()
        }).ToList();
    }

    private static List<Types.AdditionalPayment> GetAdditionalPayments(this EarningsProfileModel earningsProfile)
    {
        if (earningsProfile.AdditionalPayments == null || !earningsProfile.AdditionalPayments.Any())
        {
            return new List<Types.AdditionalPayment>();
        }

        return earningsProfile.AdditionalPayments.Select(ap => new Types.AdditionalPayment
        {
            Key = ap.Key,
            AcademicYear = ap.AcademicYear,
            DeliveryPeriod = ap.DeliveryPeriod,
            Amount = ap.Amount,
            AdditionalPaymentType = ap.AdditionalPaymentType,
            DueDate = ap.DueDate
        }).ToList();
    }

    internal static List<Types.MathsAndEnglish> GetMathsAndEnglish(this EarningsProfileModel earningsProfile)
    {
        if (earningsProfile.MathsAndEnglishCourses == null || !earningsProfile.MathsAndEnglishCourses.Any())
        {
            return new List<Types.MathsAndEnglish>();
        }

        return earningsProfile.MathsAndEnglishCourses.Select(me => new Types.MathsAndEnglish
        {
            StartDate = me.StartDate,
            EndDate = me.EndDate,
            Course = me.Course,
            Amount = me.Amount,
            Instalments = me.Instalments.Select(i => new Types.MathsAndEnglishInstalment
            {
                MathsAndEnglishKey = me.Key,
                AcademicYear = i.AcademicYear,
                DeliveryPeriod = i.DeliveryPeriod,
                Amount = i.Amount
            }).ToList()
        }).ToList();
    }
}
