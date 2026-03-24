using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;

public class GetShortCourseRequest : IQuery
{
    public Guid LearningKey { get; }
    public long Ukprn { get; }

    public GetShortCourseRequest(Guid learningKey, long ukprn)
    {
        LearningKey = learningKey;
        Ukprn = ukprn;
    }
}
