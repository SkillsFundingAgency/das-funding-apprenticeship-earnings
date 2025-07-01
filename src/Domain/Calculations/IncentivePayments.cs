using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<IncentivePayment> GenerateIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven)
    {
        var incentivePayments = new List<IncentivePayment>();
        incentivePayments.AddRange(GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate));
        incentivePayments.AddRange(Generate19To24IncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, hasEHCP, isCareLeaver, careLeaverEmployerConsentGiven));
        return incentivePayments;
    }

    public static List<IncentivePayment> GenerateUnder19sIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate)
    {
        var incentivePayments = new List<IncentivePayment>();

        if (ageAtStartOfLearning is > 18)
        {
            return incentivePayments;
        }

        if(IsEligibleFor90DayIncentive(apprenticeshipStartDate, apprenticeshipEndDate))
        {
            var incentiveDate = apprenticeshipStartDate.AddDays(89);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
        }

        if(IsEligibleFor365DayIncentive(apprenticeshipStartDate, apprenticeshipEndDate))
        {
            var incentiveDate = apprenticeshipStartDate.AddDays(364);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
        }

        return incentivePayments;
    }

    public static List<IncentivePayment> Generate19To24IncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven)
    {
        var incentivePayments = new List<IncentivePayment>();

        // Does not fit age criteria
        if (ageAtStartOfLearning is < 19 || ageAtStartOfLearning is > 24)
            return incentivePayments;

        // Is not eligible for incentive payments
        if(!hasEHCP && !isCareLeaver)
            return incentivePayments;

        if (IsEligibleFor90DayIncentive(apprenticeshipStartDate, apprenticeshipEndDate))
        {
            var incentiveDate = apprenticeshipStartDate.AddDays(89);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            if((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        if (IsEligibleFor365DayIncentive(apprenticeshipStartDate, apprenticeshipEndDate))
        {
            var incentiveDate = apprenticeshipStartDate.AddDays(364);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            if ((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        return incentivePayments;
    }

    private static bool IsEligibleFor90DayIncentive(DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate)
    {
        var duration = (apprenticeshipEndDate - apprenticeshipStartDate).Add(TimeSpan.FromDays(1));
        if (duration.Days < 90)
        {
            return false;
        }

        return true;
    }

    private static bool IsEligibleFor365DayIncentive(DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate)
    {
        var duration = (apprenticeshipEndDate - apprenticeshipStartDate).Add(TimeSpan.FromDays(1));
        if (duration.Days < 365)
        {
            return false;
        }

        return true;
    }

    private static void AddIncentivePayment(this List<IncentivePayment> incentivePayments, DateTime dueDate, decimal amount, string incentiveType)
    {
        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = dueDate.ToAcademicYear(),
            DueDate = dueDate,
            Amount = amount,
            DeliveryPeriod = dueDate.ToDeliveryPeriod(),
            IncentiveType = incentiveType
        });
    }
}
