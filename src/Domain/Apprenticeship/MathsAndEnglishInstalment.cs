using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public enum MathsAndEnglishInstalmentType
{
    Regular = 0,
    Balancing = 1
}

public class MathsAndEnglishInstalment : IDomainEntity<EnglishAndMathsInstalmentEntity>
{
    private EnglishAndMathsInstalmentEntity _entity;

    public short AcademicYear => _entity.AcademicYear;
    public byte DeliveryPeriod => _entity.DeliveryPeriod;
    public decimal Amount => _entity.Amount;
    public MathsAndEnglishInstalmentType Type => Enum.Parse<MathsAndEnglishInstalmentType>(_entity.Type);

    internal MathsAndEnglishInstalment(EnglishAndMathsInstalmentEntity entity)
    {
        _entity = entity;
    }

    public MathsAndEnglishInstalment(short academicYear, byte deliveryPeriod, decimal amount, MathsAndEnglishInstalmentType type)
    {
        _entity = new EnglishAndMathsInstalmentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            DeliveryPeriod = deliveryPeriod,
            Amount = amount,
            Type = type.ToString()
        };
    }

    public EnglishAndMathsInstalmentEntity GetModel()
    {
        return _entity;
    }

    public static MathsAndEnglishInstalment Get(EnglishAndMathsInstalmentEntity model)
    {
        return new MathsAndEnglishInstalment(model);
    }

    public bool AreSame(EnglishAndMathsInstalmentEntity? compare)
    {
        if (compare == null)
            return false;
        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount;
    }
}