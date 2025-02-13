using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<Earning> CalculateIncentivePayment(int ageAtStartOfApprenticeship, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate)
    {
        var incentivePayments = new List<Earning>();

        if(ageAtStartOfApprenticeship > 15 && ageAtStartOfApprenticeship < 19) //16-18s
        {
            var incentiveDate = apprenticeshipStartDate.AddDays(90);
            var earning = new Earning
            {
                AcademicYear = incentiveDate.ToAcademicYear(),
                DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
                Amount = 500
            };
        }

        return incentivePayments;

    }


}
