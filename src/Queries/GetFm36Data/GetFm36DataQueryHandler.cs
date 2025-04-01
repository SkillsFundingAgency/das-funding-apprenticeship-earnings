using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
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
        _logger.LogInformation("Handling GetFm36DataRequest for Ukprn: {ukprn}", query.Ukprn);

        var domainApprenticeships = _earningsQueryRepository.GetApprenticeships(query.Ukprn);

        if (domainApprenticeships == null)
        {
            _logger.LogInformation("No apprenticeships found for: {ukprn}", query.Ukprn);
            return new GetFm36DataResponse();
        }

        _logger.LogInformation("{numberOfApprenticeships} apprenticeships found for: {ukprn}", domainApprenticeships.Count, query.Ukprn);

        var apprenticeships = domainApprenticeships.Select(x => MapApprenticeship(x)).ToList();

        return new GetFm36DataResponse(apprenticeships);
    }

    private Apprenticeship MapApprenticeship(Domain.Apprenticeship.Apprenticeship source)
    {
        var currentEpisode = source.GetCurrentEpisode(_systemClockService);

        _logger.LogInformation($"source is null: {source == null}");
        _logger.LogInformation($"apprenticeship episodes is null: {source.ApprenticeshipEpisodes == null}");
        foreach (var episode in source.ApprenticeshipEpisodes)
        {
            _logger.LogInformation($"Episode {episode.ApprenticeshipEpisodeKey}");
            _logger.LogInformation($"Earnings Profile is null: {episode.EarningsProfile == null}");

            _logger.LogInformation($"Instalments is null: {episode.EarningsProfile.Instalments == null}");
            _logger.LogInformation($"Additional payments is null: {episode.EarningsProfile.AdditionalPayments == null}");
        }

        var json = JsonSerializer.Serialize(source);
        _logger.LogInformation(json);

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