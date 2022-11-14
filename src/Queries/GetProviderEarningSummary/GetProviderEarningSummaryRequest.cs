using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary
{
    public class GetProviderEarningSummaryRequest : IQuery
    {
        public string Ukprn { get; }

        public GetProviderEarningSummaryRequest(string ukprn)
        {
            Ukprn = ukprn;
        }
    }
}
