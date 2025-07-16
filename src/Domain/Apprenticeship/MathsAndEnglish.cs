using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglish : IDomainEntity<MathsAndEnglishModel>
{
    private MathsAndEnglishModel _model;
    private List<MathsAndEnglishInstalment> _instalments;

    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public string Course => _model.Course;
    public decimal Amount => _model.Amount;
    public DateTime? ActualEndDate => _model.ActualEndDate;
    public IReadOnlyCollection<MathsAndEnglishInstalment> Instalments => new ReadOnlyCollection<MathsAndEnglishInstalment>(_instalments);

    private MathsAndEnglish(MathsAndEnglishModel model)
    {
        _model = model;
        _instalments = model.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public MathsAndEnglish(DateTime startDate, DateTime endDate, string course, decimal amount, List<MathsAndEnglishInstalment> instalments, DateTime? actualEndDate)
    {
        _instalments = instalments;
        _model = new MathsAndEnglishModel();
        _model.Key = Guid.NewGuid();
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.Course = course;
        _model.Amount = amount;
        _model.ActualEndDate = actualEndDate;
        _model.Instalments = instalments.ToModels<MathsAndEnglishInstalment, MathsAndEnglishInstalmentModel>(model => model.MathsAndEnglishKey = _model.Key);
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
               Amount == compare.Amount &&
               ActualEndDate == compare.ActualEndDate &&
               Instalments.AreSame(compare.Instalments);
    }
}