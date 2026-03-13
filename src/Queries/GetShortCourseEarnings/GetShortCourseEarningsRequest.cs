using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;

public class GetShortCourseEarningsRequest : IQuery
{
    public Guid LearningKey { get; }
    public long Ukprn { get; }

    public GetShortCourseEarningsRequest(Guid learningKey, long ukprn)
    {
        LearningKey = learningKey;
        Ukprn = ukprn;
    }
}
