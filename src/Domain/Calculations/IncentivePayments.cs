using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<IncentivePayment> GenerateIncentivePayments(int ageAtStartOfApprenticeship, DateTime apprenticeshipStartDate)//, DateTime apprenticeshipEndDate)
    {
        var incentivePayments = new List<IncentivePayment>();

        //if(ageAtStartOfApprenticeship > 15 && ageAtStartOfApprenticeship < 19) //16-18s
        //{
        //    var incentiveDate = apprenticeshipStartDate.AddDays(90);
        //    var earning = new Earning
        //    {
        //        AcademicYear = incentiveDate.ToAcademicYear(),
        //        DeliveryPeriod = incentiveDate.ToDeliveryPeriod(),
        //        Amount = 500
        //    };
        //}

        //Todo: Just for fun, let's just pay £100 immediately to the provider
        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = apprenticeshipStartDate.ToAcademicYear(),
            DueDate = apprenticeshipStartDate,
            Amount = 100,
            DeliveryPeriod = apprenticeshipStartDate.ToDeliveryPeriod(),
            IncentiveType = "ProviderIncentive"
        });

        return incentivePayments;

    }


}
