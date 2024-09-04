using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;


public class GetFm36DataRequest : IQuery
{
    public long Ukprn { get; }

    public GetFm36DataRequest(long ukprn)
    {
        Ukprn = ukprn;
    }
}