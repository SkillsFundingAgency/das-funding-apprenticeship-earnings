using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;

public static class ApprenticeshipMappers
{
    public static IEnumerable<Earning>? ToEarningsReadModels(this Apprenticeship apprenticeship, ISystemClockService systemClockService)
    {
        var currentEpisode = apprenticeship.GetCurrentEpisode(systemClockService);

        return currentEpisode.EarningsProfile?.Instalments.Select(x => new Earning
        {
            Id = Guid.NewGuid(),
            AcademicYear = x.AcademicYear,
            Amount = x.Amount,
            DeliveryPeriod = x.DeliveryPeriod,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EmployerAccountId = currentEpisode.EmployerAccountId,
            FundingType = currentEpisode.FundingType,
            UKPRN = currentEpisode.UKPRN,
            Uln = apprenticeship.Uln,
            ApprenticeshipEpisodeKey = currentEpisode.ApprenticeshipEpisodeKey
        });
    }
}
