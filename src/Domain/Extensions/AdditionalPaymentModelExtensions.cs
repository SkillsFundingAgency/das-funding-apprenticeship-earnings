using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class AdditionalPaymentModelExtensions
{
    public static bool IsIncentivePayment(this AdditionalPaymentModel model)
    {
        return model.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || model.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }
}
