using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class EpisodeBreakInLearning
{
    private EpisodeBreakInLearningModel _model;

    public Guid Key => _model.Key;
    public Guid EpisodeKey => _model.EpisodeKey;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    //public int DurationInCensusDates  DONT APPROVE PR IF I DIDN'T IMPLEMENT THIS

    public EpisodeBreakInLearning(Guid episodeKey, DateTime startDate, DateTime endDate)
    {
        _model = new EpisodeBreakInLearningModel
        {
            Key = Guid.NewGuid(),
            EpisodeKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    private EpisodeBreakInLearning(EpisodeBreakInLearningModel model)
    {
        _model = model;
    }

    public static EpisodeBreakInLearning Get(EpisodeBreakInLearningModel model)
    {
        return new EpisodeBreakInLearning(model);
    }

    public EpisodeBreakInLearningModel GetModel()
    {
        return _model;
    }
}
