using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public enum MathsAndEnglishInstalmentType
{
    Regular = 0,
    Balancing = 1
}

public class MathsAndEnglishInstalment : IDomainEntity<MathsAndEnglishInstalmentModel>
{
    private MathsAndEnglishInstalmentModel _model;

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public MathsAndEnglishInstalmentType Type => Enum.Parse<MathsAndEnglishInstalmentType>(_model.Type);

    internal MathsAndEnglishInstalment(MathsAndEnglishInstalmentModel model)
    {
        _model = model;
    }

    public MathsAndEnglishInstalment(short academicYear, byte deliveryPeriod, decimal amount, MathsAndEnglishInstalmentType type)
    {
        _model = new MathsAndEnglishInstalmentModel
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            DeliveryPeriod = deliveryPeriod,
            Amount = amount,
            Type = type.ToString()
        };
    }

    public MathsAndEnglishInstalmentModel GetModel()
    {
        return _model;
    }

    public static MathsAndEnglishInstalment Get(MathsAndEnglishInstalmentModel model)
    {
        return new MathsAndEnglishInstalment(model);
    }

    public bool AreSame(MathsAndEnglishInstalmentModel? compare)
    {
        if (compare == null)
            return false;
        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount;
    }
}