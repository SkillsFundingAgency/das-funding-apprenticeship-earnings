using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

public class EnglishAndMathsPeriodInLearning
{
    private EnglishAndMathsPeriodInLearningEntity _entity;

    public Guid Key => _entity.Key;
    public Guid EnglishAndMathsKey => _entity.EnglishAndMathsKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public DateTime OriginalExpectedEndDate => _entity.OriginalExpectedEndDate;

    public EnglishAndMathsPeriodInLearning(Guid episodeKey, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        _entity = new EnglishAndMathsPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate,
            OriginalExpectedEndDate = originalExpectedEndDate
        };
    }

    private EnglishAndMathsPeriodInLearning(EnglishAndMathsPeriodInLearningEntity entity)
    {
        _entity = entity;
    }

    public static EnglishAndMathsPeriodInLearning Get(EnglishAndMathsPeriodInLearningEntity entity)
    {
        return new EnglishAndMathsPeriodInLearning(entity);
    }

    public EnglishAndMathsPeriodInLearningEntity GetEntity()
    {
        return _entity;
    }
}