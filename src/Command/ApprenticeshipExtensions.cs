using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command
{
    internal static class ApprenticeshipExtensions
    {
        internal static List<DeliveryPeriod>? BuildDeliveryPeriods(this Apprenticeship apprenticeship)
        {
            return apprenticeship.EarningsProfile?.Instalments.Select(instalment => new DeliveryPeriod
            (
                instalment.DeliveryPeriod.ToCalendarMonth(),
                instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                instalment.DeliveryPeriod,
                instalment.AcademicYear,
                instalment.Amount,
                apprenticeship.FundingLineType
            )).ToList();
        }
    }
}
