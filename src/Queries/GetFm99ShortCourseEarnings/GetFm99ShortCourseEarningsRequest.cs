using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;

public class GetFm99ShortCourseEarningsRequest : IQuery
{
    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }

    public GetFm99ShortCourseEarningsRequest(Guid learningKey, Guid episodeKey)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
    }
}
