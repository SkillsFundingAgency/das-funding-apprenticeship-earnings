using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Api.Requests;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

public class GetFm36DataRequest : IQuery, IGetApiRequest
{
    public long Ukprn { get; }

    public string GetUrl => $"{Ukprn}";

    public GetFm36DataRequest(long ukprn)
    {
        Ukprn = ukprn;
    }
}
