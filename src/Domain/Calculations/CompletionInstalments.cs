using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class CompletionInstalments
{
    public static ApprenticeshipInstalment GenerationCompletionInstalment(DateTime completionDate, decimal completionAmount, Guid priceKey)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        return new ApprenticeshipInstalment(completionYear, completionPeriod, completionAmount, priceKey, InstalmentType.Completion);
    }
}