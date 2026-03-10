using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class ShortCourseEarningsProfileExtensions
{
    internal static ShortCourseEarningsProfileUpdatedEvent CreatedEarningsProfileUpdatedEvent(this ShortCourseEarningsProfileEntity earningsProfile, bool initialGeneration = false)
    {
        var archiveEarningsProfileEvent = new ShortCourseEarningsProfileUpdatedEvent
        {
            EarningsProfileId = earningsProfile.EarningsProfileId,
            EpisodeKey = earningsProfile.EpisodeKey,
            Version = earningsProfile.Version,
            OnProgramTotal = earningsProfile.OnProgramTotal,
            CompletionPayment = earningsProfile.CompletionPayment,
            Instalments = earningsProfile.GetInstalments(),
            InitialGeneration = initialGeneration
        };

        return archiveEarningsProfileEvent;
    }

    private static List<Instalment> GetInstalments(this ShortCourseEarningsProfileEntity earningsProfile)
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
            Type = i.Type
        }).OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod).ToList();
    }
}
