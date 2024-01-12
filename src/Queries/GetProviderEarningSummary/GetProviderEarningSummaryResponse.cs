using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary
{
    public class GetProviderEarningSummaryResponse
    {
        public GetProviderEarningSummaryResponse(ProviderEarningsSummary providerEarningsSummary)
        {
            ProviderEarningsSummary = providerEarningsSummary;
        }

        public DataTransferObjects.ProviderEarningsSummary ProviderEarningsSummary { get; }
    }
}
