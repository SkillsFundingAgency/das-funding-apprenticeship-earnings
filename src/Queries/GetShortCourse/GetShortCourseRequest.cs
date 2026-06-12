using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;

public class GetShortCourseRequest : IQuery
{
    public Guid LearningKey { get; }
    public Guid EpisodeKey { get; }

    public GetShortCourseRequest(Guid learningKey, Guid episodeKey)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
    }
}
