using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;

public class GetFm99ShortCourseEarningsRequest : IQuery
{
    public Guid LearningKey { get; }
    public long Ukprn { get; }

    public GetFm99ShortCourseEarningsRequest(Guid learningKey, long ukprn)
    {
        LearningKey = learningKey;
        Ukprn = ukprn;
    }
}
