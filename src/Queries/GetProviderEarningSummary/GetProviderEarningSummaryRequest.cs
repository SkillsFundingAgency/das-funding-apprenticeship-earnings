using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary
{
    public class GetProviderEarningSummaryRequest : IQuery
    {
        public long Ukprn { get; }

        public GetProviderEarningSummaryRequest(long ukprn)
        {
            Ukprn = ukprn;
        }
    }
}
