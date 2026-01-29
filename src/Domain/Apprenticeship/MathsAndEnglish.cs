using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglish : IDomainEntity<MathsAndEnglishModel>
{
    private MathsAndEnglishModel _model;
    private List<MathsAndEnglishInstalment> _instalments;

    public Guid Key => _model.Key;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public string Course => _model.Course;
    public string LearnAimRef => _model.LearnAimRef;
    public decimal Amount => _model.Amount;
    public DateTime? WithdrawalDate => _model.WithdrawalDate;
    public DateTime? CompletionDate => _model.CompletionDate;
    public DateTime? ActualEndDate => WithdrawalDate ?? CompletionDate ?? PauseDate;
    public DateTime? PauseDate => _model.PauseDate;
    public int? PriorLearningAdjustmentPercentage => _model.PriorLearningAdjustmentPercentage;
    public IReadOnlyCollection<MathsAndEnglishInstalment> Instalments => new ReadOnlyCollection<MathsAndEnglishInstalment>(_instalments);
    public IReadOnlyCollection<MathsAndEnglishPeriodInLearning> PeriodsInLearning => new ReadOnlyCollection<MathsAndEnglishPeriodInLearning>(_model.PeriodsInLearning.Select(MathsAndEnglishPeriodInLearning.Get).ToList());

    private MathsAndEnglish(MathsAndEnglishModel model)
    {
        _model = model;
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
        _model = new MathsAndEnglishModel();
        _model.Key = Guid.NewGuid();
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.Course = course;
        _model.LearnAimRef = learnAimRef;
        _model.Amount = amount;
        _model.CompletionDate = completionDate;
        _model.WithdrawalDate = withdrawalDate;
        _model.PauseDate = pauseDate;
        _model.PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage;
        _model.PeriodsInLearning = periodsInLearning.Select(pil => new MathsAndEnglishPeriodInLearningModel
        {
            Key = Guid.NewGuid(),
            MathsAndEnglishKey = _model.Key,
            StartDate = pil.StartDate,
            EndDate = pil.EndDate,
            OriginalExpectedEndDate = pil.OriginalExpectedEndDate
        }).ToList();
        _model.Instalments = EnglishAndMathsPayments.GenerateInstalments(this);

        _instalments = _model.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public MathsAndEnglishModel GetModel()
    {
        return _model;
    }

    public static MathsAndEnglish Get(MathsAndEnglishModel model)
    {
        return new MathsAndEnglish(model);
    }

    public bool AreSame(MathsAndEnglishModel? compare)
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