using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglish : IDomainEntity<EnglishAndMathsEntity>
{
    private EnglishAndMathsEntity _entity;
    private List<MathsAndEnglishInstalment> _instalments;

    public Guid Key => _entity.Key;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public string Course => _entity.Course;
    public string LearnAimRef => _entity.LearnAimRef;
    public decimal Amount => _entity.Amount;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public DateTime? ActualEndDate => WithdrawalDate ?? CompletionDate ?? PauseDate;
    public DateTime? PauseDate => _entity.PauseDate;
    public int? PriorLearningAdjustmentPercentage => _entity.PriorLearningAdjustmentPercentage;
    public IReadOnlyCollection<MathsAndEnglishInstalment> Instalments => new ReadOnlyCollection<MathsAndEnglishInstalment>(_instalments);
    public IReadOnlyCollection<MathsAndEnglishPeriodInLearning> PeriodsInLearning => new ReadOnlyCollection<MathsAndEnglishPeriodInLearning>(_entity.PeriodsInLearning.Select(MathsAndEnglishPeriodInLearning.Get).ToList());

    private MathsAndEnglish(EnglishAndMathsEntity model)
    {
        _entity = model;
        _instalments = model.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public MathsAndEnglish(
        DateTime startDate, 
        DateTime endDate, 
        string course, 
        string learnAimRef, 
        decimal amount, 
        DateTime? withdrawalDate, 
        DateTime? completionDate,
        DateTime? pauseDate, 
        int? priorLearningAdjustmentPercentage,
        IEnumerable<IPeriodInLearning> periodsInLearning)
    {
        _entity = new EnglishAndMathsEntity();
        _entity.Key = Guid.NewGuid();
        _entity.StartDate = startDate;
        _entity.EndDate = endDate;
        _entity.Course = course;
        _entity.LearnAimRef = learnAimRef;
        _entity.Amount = amount;
        _entity.CompletionDate = completionDate;
        _entity.WithdrawalDate = withdrawalDate;
        _entity.PauseDate = pauseDate;
        _entity.PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage;
        _entity.PeriodsInLearning = periodsInLearning.Select(pil => new EnglishAndMathsPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = _entity.Key,
            StartDate = pil.StartDate,
            EndDate = pil.EndDate,
            OriginalExpectedEndDate = pil.OriginalExpectedEndDate
        }).ToList();
        _entity.Instalments = EnglishAndMathsPayments.GenerateInstalments(this);

        _instalments = _entity.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public EnglishAndMathsEntity GetModel()
    {
        return _entity;
    }

    public static MathsAndEnglish Get(EnglishAndMathsEntity model)
    {
        return new MathsAndEnglish(model);
    }

    public bool AreSame(EnglishAndMathsEntity? compare)
    {
        if (compare == null)
            return false;

        return StartDate == compare.StartDate &&
               EndDate == compare.EndDate &&
               Course == compare.Course &&
               LearnAimRef == compare.LearnAimRef &&
               Amount == compare.Amount &&
               CompletionDate == compare.CompletionDate &&
               WithdrawalDate == compare.WithdrawalDate &&
               PauseDate == compare.PauseDate &&
               PriorLearningAdjustmentPercentage == compare.PriorLearningAdjustmentPercentage &&
               Instalments.AreSame(compare.Instalments);
    }
}