using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public enum InstalmentType
{
    Regular = 0,
    Completion = 1,
    Balancing = 2
}

public class Instalment : IDomainEntity<InstalmentModel>
{
    private InstalmentModel _model;

    public Instalment(short academicYear, byte deliveryPeriod, decimal amount, Guid priceKey, InstalmentType instalmentType = InstalmentType.Regular)
    {
        _model = new InstalmentModel
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            DeliveryPeriod = deliveryPeriod,
            Amount = amount,
            EpisodePriceKey = priceKey,
            Type = instalmentType.ToString()
        };
    }

    private Instalment(InstalmentModel model)
    {
        _model = model;
    }

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public Guid EpisodePriceKey => _model.EpisodePriceKey;
    public InstalmentType Type => Enum.Parse<InstalmentType>(_model.Type);

    public InstalmentModel GetModel()
    {
        return _model;
    }

    public static Instalment Get(InstalmentModel model)
    {
        return new Instalment(model);
    }

    public bool AreSame(InstalmentModel? compare)
    {
        if (compare == null)
            return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount &&
               EpisodePriceKey == compare.EpisodePriceKey;
    }
}
