using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class ApprenticeshipFactory : IApprenticeshipFactory
{
    public Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum)
    {
        var model = new ApprenticeshipModel
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            Key = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode, fundingBandMaximum) },
            DateOfBirth = learningCreatedEvent.DateOfBirth
        };

        return Apprenticeship.Apprenticeship.Get(model);
    }

    public Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model)
    {
        return Apprenticeship.Apprenticeship.Get(model);
    }
}
