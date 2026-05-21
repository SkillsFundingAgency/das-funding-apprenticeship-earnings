using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

public class EnglishAndMaths : IDomainEntity<EnglishAndMathsEntity>
{
    private EnglishAndMathsEntity _entity;
    private List<EnglishAndMathsInstalment> _instalments;

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
    public decimal? CombinedFundingAdjustmentPercentage => _entity.CombinedFundingAdjustmentPercentage;
    public IReadOnlyCollection<EnglishAndMathsInstalment> Instalments => new ReadOnlyCollection<EnglishAndMathsInstalment>(_instalments);
    public IReadOnlyCollection<EnglishAndMathsPeriodInLearning> PeriodsInLearning => new ReadOnlyCollection<EnglishAndMathsPeriodInLearning>(_entity.PeriodsInLearning.Select(EnglishAndMathsPeriodInLearning.Get).ToList());

    private EnglishAndMaths(EnglishAndMathsEntity entity)
    {
        _entity = entity;
        _instalments = entity.Instalments.Select(EnglishAndMathsInstalment.Get).ToList();
    }

    public EnglishAndMaths(
        DateTime startDate, 
        DateTime endDate, 
        string course, 
        string learnAimRef, 
        decimal amount, 
        DateTime? withdrawalDate, 
        DateTime? completionDate,
        DateTime? pauseDate,
        decimal? combinedFundingAdjustmentPercentage,
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
        _entity.CombinedFundingAdjustmentPercentage = combinedFundingAdjustmentPercentage;
        _entity.PeriodsInLearning = periodsInLearning.Select(pil => new EnglishAndMathsPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = _entity.Key,
            StartDate = pil.StartDate,
            EndDate = pil.EndDate,
            OriginalExpectedEndDate = pil.OriginalExpectedEndDate
        }).ToList();
        _entity.Instalments = EnglishAndMathsPayments.GenerateInstalments(this);

        _instalments = _entity.Instalments.Select(EnglishAndMathsInstalment.Get).ToList();
    }

    public EnglishAndMathsEntity GetEntity()
    {
        return _entity;
    }

    public static EnglishAndMaths Get(EnglishAndMathsEntity model)
    {
        return new EnglishAndMaths(model);
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
               CombinedFundingAdjustmentPercentage == compare.CombinedFundingAdjustmentPercentage &&
               Instalments.AreSame(compare.Instalments);
    }
}