using PaymentsEmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public static class EmployerTypeExtensions
{
    public static PaymentsEmployerType ToPaymentsEmployerType(this EmployerType employerType)
    {
        return employerType == EmployerType.Levy ? PaymentsEmployerType.Levy : PaymentsEmployerType.NonLevy;
    }
}
