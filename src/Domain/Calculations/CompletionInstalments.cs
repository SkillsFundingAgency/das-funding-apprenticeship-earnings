using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class CompletionInstalments
{
    public static Instalment GenerationCompletionInstalment(DateTime completionDate, decimal completionAmount, Guid priceKey)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        return new Instalment(completionYear, completionPeriod, completionAmount, priceKey, InstalmentType.Completion);
    }
}