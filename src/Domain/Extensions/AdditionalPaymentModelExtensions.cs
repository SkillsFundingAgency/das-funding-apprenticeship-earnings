using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class AdditionalPaymentModelExtensions
{
    public static bool IsIncentivePayment(this AdditionalPaymentModel model)
    {
        return model.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || model.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }

    public static bool IsIncentivePayment(this AdditionalPayment model)
    {
        return model.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || model.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }
}
