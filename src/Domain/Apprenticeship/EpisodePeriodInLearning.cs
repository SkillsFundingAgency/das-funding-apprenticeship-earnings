using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class EpisodePeriodInLearning
{
    private EpisodePeriodInLearningModel _model;

    public Guid Key => _model.Key;
    public Guid EpisodeKey => _model.EpisodeKey;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public DateTime OriginalExpectedEndDate => _model.OriginalExpectedEndDate;
    public int DurationInCensusDates => StartDate.NumberOfCensusDates(EndDate);
    public int Duration => (_model.EndDate - _model.StartDate).Days + 1;

    public EpisodePeriodInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        _model = new EpisodePeriodInLearningModel
        {
            Key = Guid.NewGuid(),
            EpisodeKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate,
            OriginalExpectedEndDate = originalExpectedEndDate
        };
    }

    private EpisodePeriodInLearning(EpisodePeriodInLearningModel model)
    {
        _model = model;
    }

    public static EpisodePeriodInLearning Get(EpisodePeriodInLearningModel model)
    {
        return new EpisodePeriodInLearning(model);
    }

    public EpisodePeriodInLearningModel GetModel()
    {
        return _model;
    }
}