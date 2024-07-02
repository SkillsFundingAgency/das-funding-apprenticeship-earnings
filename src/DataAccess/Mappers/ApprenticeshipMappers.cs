using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;

internal static class ApprenticeshipMappers
{
    internal static IEnumerable<Earning>? ToEarningsReadModels(this Apprenticeship apprenticeship)
    {
        //todo fix this in FLP-800
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.FirstOrDefault(); // DO NOT COMMIT THIS LINE, USE ISYSTEM CLOCK

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
            Uln = apprenticeship.Uln
        });
    }
}
