using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command
{
    internal static class ApprenticeshipExtensions
    {
        internal static List<DeliveryPeriod> BuildDeliveryPeriods(this Apprenticeship apprenticeship)
        {
            return apprenticeship.EarningsProfile.Instalments.Select(instalment => new DeliveryPeriod
            {
                Period = instalment.DeliveryPeriod,
                CalendarMonth = instalment.DeliveryPeriod.ToCalendarMonth(),
                CalenderYear = instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                AcademicYear = instalment.AcademicYear,
                LearningAmount = instalment.Amount,
                FundingLineType = apprenticeship.FundingLineType
            }).ToList();
        }
    }
}
