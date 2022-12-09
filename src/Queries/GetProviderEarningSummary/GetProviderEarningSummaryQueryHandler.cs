using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary
{
    public class GetProviderEarningSummaryQueryHandler : IQueryHandler<GetProviderEarningSummaryRequest, GetProviderEarningSummaryResponse>
    {
        private readonly IEarningsQueryRepository _earningsQueryRepository;
        private readonly IAcademicYearService _academicYearService;

        public GetProviderEarningSummaryQueryHandler(IEarningsQueryRepository earningsQueryRepository, IAcademicYearService academicYearService)
        {
            _earningsQueryRepository = earningsQueryRepository;
            _academicYearService = academicYearService;
        }

        public async Task<GetProviderEarningSummaryResponse> Handle(GetProviderEarningSummaryRequest query, CancellationToken cancellationToken = default)
        {
            var providerEarningsSummary = await _earningsQueryRepository.GetProviderSummary(query.Ukprn, _academicYearService.CurrentAcademicYear);
            
            var response = new GetProviderEarningSummaryResponse { ProviderEarningsSummary = providerEarningsSummary };

            return await Task.FromResult(response);
        }
    }
}
