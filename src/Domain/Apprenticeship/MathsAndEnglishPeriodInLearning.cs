using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglishPeriodInLearning
{
    private EnglishAndMathsPeriodInLearningEntity _model;

    public Guid Key => _model.Key;
    public Guid MathsAndEnglishKey => _model.EnglishAndMathsKey;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public DateTime OriginalExpectedEndDate => _model.OriginalExpectedEndDate;

    public MathsAndEnglishPeriodInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        _model = new EnglishAndMathsPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate,
            OriginalExpectedEndDate = originalExpectedEndDate
        };
    }

    private MathsAndEnglishPeriodInLearning(EnglishAndMathsPeriodInLearningEntity model)
    {
        _model = model;
    }

    public static MathsAndEnglishPeriodInLearning Get(EnglishAndMathsPeriodInLearningEntity model)
    {
        return new MathsAndEnglishPeriodInLearning(model);
    }

    public EnglishAndMathsPeriodInLearningEntity GetModel()
    {
        return _model;
    }
}