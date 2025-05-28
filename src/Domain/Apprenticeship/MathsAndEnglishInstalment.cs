using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class MathsAndEnglishInstalment
{
    private MathsAndEnglishInstalmentModel _model;

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;

    internal MathsAndEnglishInstalment(MathsAndEnglishInstalmentModel model)
    {
        _model = model;
    }

    public MathsAndEnglishInstalment(short academicYear, byte deliveryPeriod, decimal amount)
    {
        _model = new MathsAndEnglishInstalmentModel
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            DeliveryPeriod = deliveryPeriod,
            Amount = amount
        };
    }

    public MathsAndEnglishInstalmentModel GetModel(Guid mathsAndEnglishKey)
    {
        _model.MathsAndEnglishKey = mathsAndEnglishKey;
        return _model;
    }

    public static MathsAndEnglishInstalment Get(MathsAndEnglishInstalmentModel model)
    {
        return new MathsAndEnglishInstalment(model);
    }
}