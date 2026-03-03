using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class EpisodePeriodInLearning
{
    private ApprenticeshipPeriodInLearningEntity _entity;

    public Guid Key => _entity.Key;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public DateTime OriginalExpectedEndDate => _entity.OriginalExpectedEndDate;
    public int DurationInCensusDates => StartDate.NumberOfCensusDates(EndDate);
    public int Duration => (_entity.EndDate - _entity.StartDate).Days + 1;

    public EpisodePeriodInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        _entity = new ApprenticeshipPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            EpisodeKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate,
            OriginalExpectedEndDate = originalExpectedEndDate
        };
    }

    private EpisodePeriodInLearning(ApprenticeshipPeriodInLearningEntity model)
    {
        _entity = model;
    }

    public static EpisodePeriodInLearning Get(ApprenticeshipPeriodInLearningEntity model)
    {
        return new EpisodePeriodInLearning(model);
    }

    public ApprenticeshipPeriodInLearningEntity GetModel()
    {
        return _entity;
    }
}