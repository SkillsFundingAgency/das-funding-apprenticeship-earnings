using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

public class GetFm36DataQueryHandler : IQueryHandler<GetFm36DataRequest, GetFm36DataResponse>
{
    private readonly IEarningsQueryRepository _earningsQueryRepository;
    private readonly ISystemClockService _systemClockService;
    private readonly ILogger<GetFm36DataQueryHandler> _logger;

    public GetFm36DataQueryHandler(IEarningsQueryRepository earningsQueryRepository, ISystemClockService systemClockService, ILogger<GetFm36DataQueryHandler> logger)
    {
        _earningsQueryRepository = earningsQueryRepository;
        _systemClockService = systemClockService;
        _logger = logger;
    }

    public async Task<GetFm36DataResponse> Handle(GetFm36DataRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetFm36DataRequest for Ukprn: {ukprn} Year:{collectionYear} Period:{collectionPeriod} LearningKey:{learningKey}", query.Ukprn, query.CollectionYear, query.CollectionPeriod, query.LearningKey);

        var domainApprenticeship = _earningsQueryRepository.GetApprenticeship(query.LearningKey, query.Ukprn, query.CollectionYear.ToDateTime(query.CollectionPeriod), true);

        if (domainApprenticeship == null)
        {
            _logger.LogInformation("No apprenticeships found for: {ukprn}", query.Ukprn);
            return new GetFm36DataResponse();
        }

        var apprenticeship = MapApprenticeship(domainApprenticeship);

        return new GetFm36DataResponse { Apprenticeship = apprenticeship };
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
                    Amount = i.Amount,
                    EpisodePriceKey = i.EpisodePriceKey,
                    InstalmentType = i.Type.ToString()
                }).ToList(),
                AdditionalPayments = x.EarningsProfile!.AdditionalPayments.Select(p => new AdditionalPayment
                {
                    AcademicYear = p.AcademicYear,
                    DeliveryPeriod = p.DeliveryPeriod,
                    Amount = p.Amount,
                    AdditionalPaymentType = p.AdditionalPaymentType,
                    DueDate = p.DueDate
                }).ToList(),
                CompletionPayment = x.EarningsProfile.CompletionPayment,
                OnProgramTotal = x.EarningsProfile.OnProgramTotal
            }).ToList(),
            FundingLineType = currentEpisode.FundingLineType.ToString()
        };
    }
}