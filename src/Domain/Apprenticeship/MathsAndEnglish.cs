using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
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
    public DateTime? ActualEndDate => _model.ActualEndDate;
    public DateTime? PauseDate => _model.PauseDate;
    public int? PriorLearningAdjustmentPercentage => _model.PriorLearningAdjustmentPercentage;
    public DateTime? LastDayOfCourse => GetLastDayOfCourse();
    public IReadOnlyCollection<MathsAndEnglishInstalment> Instalments => new ReadOnlyCollection<MathsAndEnglishInstalment>(_instalments);

    private MathsAndEnglish(MathsAndEnglishModel model)
    {
        _model = model;
        _instalments = model.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public MathsAndEnglish(DateTime startDate, DateTime endDate, string course, string learnAimRef, decimal amount, DateTime? withdrawalDate, DateTime? actualEndDate, DateTime? pauseDate, int? priorLearningAdjustmentPercentage)
    {
        _model = new MathsAndEnglishModel();
        _model.Key = Guid.NewGuid();
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.Course = course;
        _model.LearnAimRef = learnAimRef;
        _model.Amount = amount;
        _model.WithdrawalDate = withdrawalDate;
        _model.ActualEndDate = actualEndDate;
        _model.PauseDate = pauseDate;
        _model.PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage;
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
               WithdrawalDate == compare.WithdrawalDate &&
               ActualEndDate == compare.ActualEndDate &&
               PriorLearningAdjustmentPercentage == compare.PriorLearningAdjustmentPercentage &&
               Instalments.AreSame(compare.Instalments);
    }

    private DateTime? GetLastDayOfCourse()
    {
        var plausibleLastDaysOfLearning = new List<DateTime?>()
        {
            ActualEndDate,
            WithdrawalDate,
            PauseDate
        };

        return plausibleLastDaysOfLearning
            .Where(d => d.HasValue)
            .OrderBy(d => d!.Value)
            .FirstOrDefault();
    }

}