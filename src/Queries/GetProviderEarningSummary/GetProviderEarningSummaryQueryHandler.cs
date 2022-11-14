using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary
{
    public class GetProviderEarningSummaryQueryHandler : IQueryHandler<GetProviderEarningSummaryRequest, GetProviderEarningSummaryResponse>
    {
        private readonly IEarningsQueryRepository _earningsQueryRepository;

        public GetProviderEarningSummaryQueryHandler(IEarningsQueryRepository earningsQueryRepository)
        {
            _earningsQueryRepository = earningsQueryRepository;
        }

        public async Task<GetProviderEarningSummaryResponse> Handle(GetProviderEarningSummaryRequest query, CancellationToken cancellationToken = default)
        {
            var providerEarningsSummary = await _earningsQueryRepository.GetProviderSummary(query.Ukprn);
            
            var response = new GetProviderEarningSummaryResponse { ProviderEarningsSummary = providerEarningsSummary };

            return await Task.FromResult(response);
        }
    }
}
