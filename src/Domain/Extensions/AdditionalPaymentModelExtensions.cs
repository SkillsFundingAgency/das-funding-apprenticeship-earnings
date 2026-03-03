using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class AdditionalPaymentModelExtensions
{
    public static bool IsIncentivePayment(this ApprenticeshipAdditionalPaymentEntity model)
    {
        return model.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || model.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }

    public static bool IsIncentivePayment(this AdditionalPayment model)
    {
        return model.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || model.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }
}
