using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers
{
    internal static class ApprenticeshipMappers
    {
        internal static IEnumerable<Earning> ToEarningsReadModels(this Apprenticeship apprenticeship)
        {
            return apprenticeship.EarningsProfile.Instalments.Select(x => new Earning
            {
                Id = Guid.NewGuid(),
                AcademicYear = x.AcademicYear,
                Amount = x.Amount,
                DeliveryPeriod = x.DeliveryPeriod,
                ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
                ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
                EmployerAccountId = apprenticeship.EmployerAccountId,
                FundingEmployerAccountId = apprenticeship.FundingEmployerAccountId,
                FundingType = apprenticeship.FundingType,
                UKPRN = apprenticeship.UKPRN,
                Uln = apprenticeship.Uln
            });
        }
    }
}
