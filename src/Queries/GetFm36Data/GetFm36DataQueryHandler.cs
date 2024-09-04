using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

public class GetFm36DataQueryHandler : IQueryHandler<GetFm36DataRequest, GetFm36DataResponse>
{
    private readonly IEarningsQueryRepository _earningsQueryRepository;
    private readonly ISystemClockService _systemClockService;

    public GetFm36DataQueryHandler(IEarningsQueryRepository earningsQueryRepository, ISystemClockService systemClockService)
    {
        _earningsQueryRepository = earningsQueryRepository;
        _systemClockService = systemClockService;
    }

    public async Task<GetFm36DataResponse> Handle(GetFm36DataRequest query, CancellationToken cancellationToken = default)
    {
        var domainApprenticeships = _earningsQueryRepository.GetApprenticeships(query.Ukprn);

        if (domainApprenticeships == null)
            return new GetFm36DataResponse();

        var apprenticeships = domainApprenticeships.Select(x => MapApprenticeship(x)).ToList();

        return new GetFm36DataResponse(apprenticeships);
    }

    private Apprenticeship MapApprenticeship(Domain.Apprenticeship.Apprenticeship source)
    {
        var currentEpisode = source.GetCurrentEpisode(_systemClockService);

        return new Apprenticeship
        {
            Key = source.ApprenticeshipKey,
            Ukprn = currentEpisode.UKPRN,
            Episodes = source.ApprenticeshipEpisodes.Select(x => new Episode
            {
                Key = x.ApprenticeshipEpisodeKey,
                NumberOfInstalments = x.EarningsProfile!.Instalments.Count,
                Instalments = x.EarningsProfile.Instalments.Select(i => new Instalment
                {
                    AcademicYear = i.AcademicYear,
                    DeliveryPeriod = i.DeliveryPeriod,
                    Amount = i.Amount
                }).ToList(),
                CompletionPayment = x.EarningsProfile.CompletionPayment,
                OnProgramTotal = x.EarningsProfile.OnProgramTotal
            }).ToList(),
            FundingLineType = currentEpisode.FundingType.ToString()
        };
    }
}