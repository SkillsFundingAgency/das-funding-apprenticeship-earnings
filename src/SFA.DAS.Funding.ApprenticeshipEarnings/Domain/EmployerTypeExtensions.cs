namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public static class EmployerTypeExtensions
{
    public static Events.EmployerType ToOutboundEventEmployerType(this InternalEvents.EmployerType employerType)
    {
        if (employerType == InternalEvents.EmployerType.NonLevy)
            return Events.EmployerType.NonLevy;

        return Events.EmployerType.Levy;
    }
}