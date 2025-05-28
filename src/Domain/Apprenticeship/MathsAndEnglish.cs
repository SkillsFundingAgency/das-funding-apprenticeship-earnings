using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglish
{
    private MathsAndEnglishModel _model;
    private List<MathsAndEnglishInstalment> _instalments;

    public DateTime StartDate => _model.StartDate;
    public DateTime? EndDate => _model.EndDate;
    public string Course => _model.Course;
    public decimal Amount => _model.Amount;
    public IReadOnlyCollection<MathsAndEnglishInstalment> Instalments => new ReadOnlyCollection<MathsAndEnglishInstalment>(_instalments);

    private MathsAndEnglish(MathsAndEnglishModel model)
    {
        _model = model;
        _instalments = model.Instalments.Select(MathsAndEnglishInstalment.Get).ToList();
    }

    public MathsAndEnglish(DateTime startDate, DateTime? endDate, string course, decimal amount, List<MathsAndEnglishInstalment> instalments)
    {
        _instalments = instalments;
        _model = new MathsAndEnglishModel();
        _model.Key = Guid.NewGuid();
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.Course = course;
        _model.Amount = amount;
        _model.Instalments = instalments.Select(x => x.GetModel(_model.Key)).ToList();
    }

    public MathsAndEnglishModel GetModel(Guid earningsProfileKey)
    {
        _model.EarningsProfileId = earningsProfileKey;
        return _model;
    }

    public static MathsAndEnglish Get(MathsAndEnglishModel model)
    {
        return new MathsAndEnglish(model);
    }
}