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
        try
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
                    Instalments = GetInstalmentsFromEpisode(x),
                    CompletionPayment = x.EarningsProfile.CompletionPayment,
                    OnProgramTotal = x.EarningsProfile.OnProgramTotal,
                    AdditionalPayments = x.EarningsProfile!.AdditionalPayments.Select(p => new AdditionalPayment
                    {
                        AcademicYear = p.AcademicYear,
                        DeliveryPeriod = p.DeliveryPeriod,
                        Amount = p.Amount,
                        AdditionalPaymentType = p.AdditionalPaymentType,
                        DueDate = p.DueDate
                    }).ToList()
                }).ToList(),
                FundingLineType = currentEpisode.FundingLineType.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error mapping apprenticeship: {apprenticeshipKey} message:{innerMsg}", source.ApprenticeshipKey, ex.Message);
            throw;
        }

    }

    // At the moment this throws exceptions if an episode is missing instalments
    // if later we there are scenarios where this is valid we can change this to return an empty list
    private static List<Instalment> GetInstalmentsFromEpisode(ApprenticeshipEpisode apprenticeshipEpisode)
    {
        if(apprenticeshipEpisode.EarningsProfile == null)
            throw new InvalidOperationException("Cannot GetInstalmentsFromEpisode EarningsProfile is null");

        if(apprenticeshipEpisode.EarningsProfile.Instalments == null)
            throw new InvalidOperationException("Cannot GetInstalmentsFromEpisode instalments are null");

        return apprenticeshipEpisode.EarningsProfile.Instalments.Select(i => new Instalment
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount
        }).ToList();

    }
}