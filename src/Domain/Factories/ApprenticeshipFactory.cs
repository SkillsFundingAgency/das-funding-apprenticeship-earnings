using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class ApprenticeshipFactory : IApprenticeshipFactory
{
    public Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum)
    {
        var model = new LearningModel
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            LearningKey = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode, fundingBandMaximum) },
            DateOfBirth = learningCreatedEvent.DateOfBirth
        };

        return Apprenticeship.Apprenticeship.Get(model);
    }

    public Apprenticeship.Apprenticeship GetExisting(LearningModel model)
    {
        return Apprenticeship.Apprenticeship.Get(model);
    }
}
