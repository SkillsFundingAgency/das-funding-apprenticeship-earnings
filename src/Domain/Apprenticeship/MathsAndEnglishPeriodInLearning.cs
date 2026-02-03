using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglishPeriodInLearning
{
    private MathsAndEnglishPeriodInLearningModel _model;

    public Guid Key => _model.Key;
    public Guid MathsAndEnglishKey => _model.MathsAndEnglishKey;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public DateTime OriginalExpectedEndDate => _model.OriginalExpectedEndDate;

    public MathsAndEnglishPeriodInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        _model = new MathsAndEnglishPeriodInLearningModel
        {
            Key = Guid.NewGuid(),
            MathsAndEnglishKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate,
            OriginalExpectedEndDate = originalExpectedEndDate
        };
    }

    private MathsAndEnglishPeriodInLearning(MathsAndEnglishPeriodInLearningModel model)
    {
        _model = model;
    }

    public static MathsAndEnglishPeriodInLearning Get(MathsAndEnglishPeriodInLearningModel model)
    {
        return new MathsAndEnglishPeriodInLearning(model);
    }

    public MathsAndEnglishPeriodInLearningModel GetModel()
    {
        return _model;
    }
}