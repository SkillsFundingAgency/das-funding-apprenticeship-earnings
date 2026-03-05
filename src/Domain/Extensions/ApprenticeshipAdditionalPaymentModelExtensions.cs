using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

internal static class ApprenticeshipAdditionalPaymentModelExtensions
{
    public static bool IsIncentivePayment(this ApprenticeshipAdditionalPaymentEntity entity)
    {
        return entity.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || entity.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }

    public static bool IsIncentivePayment(this AdditionalPayment entity)
    {
        return entity.AdditionalPaymentType == InstalmentTypes.EmployerIncentive || entity.AdditionalPaymentType == InstalmentTypes.ProviderIncentive;
    }
}
