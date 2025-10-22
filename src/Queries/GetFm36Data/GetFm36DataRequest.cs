using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;


public class GetFm36DataRequest : IQuery
{
    public long Ukprn { get; }
    public short CollectionYear { get; }
    public byte CollectionPeriod { get; }
    public List<Guid> LearningKeys { get; }


    public GetFm36DataRequest(long ukprn, short collectionYear, byte collectionPeriod, List<Guid> learningKeys)
    {
        Ukprn = ukprn;
        CollectionYear = collectionYear;
        CollectionPeriod = collectionPeriod;
        LearningKeys = learningKeys;
    }
}

internal static class GetFm36DataRequestExtensions
{
    internal static string LearningKeysLogInfo(this GetFm36DataRequest request)
    {
        if(request.LearningKeys == null || !request.LearningKeys.Any())
        {
            return "no learning keys provided for query";
        }

        return $"{request.LearningKeys.Count} learning keys provided";
    }
}