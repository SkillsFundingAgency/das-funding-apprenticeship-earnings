using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

//public class EpisodeBreakInLearning
//{
//    private EpisodeBreakInLearningModel _model;

//    public Guid Key => _model.Key;
//    public Guid EpisodeKey => _model.EpisodeKey;
//    public DateTime StartDate => _model.StartDate;
//    public DateTime EndDate => _model.EndDate;
//    public DateTime PriorPeriodExpectedEndDate => _model.PriorPeriodExpectedEndDate;
//    public int DurationInCensusDates  => StartDate.NumberOfCensusDates(EndDate);
//    public int Duration => (_model.EndDate - _model.StartDate).Days + 1;

//    public EpisodeBreakInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime priorPeriodExpectedEndDate)
//    {
//        _model = new EpisodeBreakInLearningModel
//        {
//            Key = Guid.NewGuid(),
//            EpisodeKey = episodeKey,
//            StartDate = startDate,
//            EndDate = endDate,
//            PriorPeriodExpectedEndDate = priorPeriodExpectedEndDate
//        };
//    }

//    private EpisodeBreakInLearning(EpisodeBreakInLearningModel model)
//    {
//        _model = model;
//    }

//    public static EpisodeBreakInLearning Get(EpisodeBreakInLearningModel model)
//    {
//        return new EpisodeBreakInLearning(model);
//    }

//    public EpisodeBreakInLearningModel GetModel()
//    {
//        return _model;
//    }
//}