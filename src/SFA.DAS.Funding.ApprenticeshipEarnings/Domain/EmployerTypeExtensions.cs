using SFA.DAS.Apprenticeships.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public static class FundingTypeExtensions
{
    public static Events.EmployerType ToOutboundEventEmployerType(this FundingType fundingType)
    {
        if (fundingType == FundingType.NonLevy)
            return Events.EmployerType.NonLevy;

        return Events.EmployerType.Levy;
    }
}