using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public static class ApprenticeshipExtensions
{
    public static List<DeliveryPeriod>? BuildDeliveryPeriods(this ApprenticeshipEpisode currentEpisode)
    {

        return currentEpisode.EarningsProfile?.Instalments.Select(instalment => new DeliveryPeriod
        (
            instalment.DeliveryPeriod.ToCalendarMonth(),
            instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
            instalment.DeliveryPeriod,
            instalment.AcademicYear,
            instalment.Amount,
            currentEpisode.FundingLineType
        )).ToList();
    }
}