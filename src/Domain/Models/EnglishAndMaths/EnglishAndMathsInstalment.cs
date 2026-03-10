using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

public enum EnglishAndMathsInstalmentType
{
    Regular = 0,
    Balancing = 1
}

public class EnglishAndMathsInstalment : IDomainEntity<EnglishAndMathsInstalmentEntity>
{
    private EnglishAndMathsInstalmentEntity _entity;

    public short AcademicYear => _entity.AcademicYear;
    public byte DeliveryPeriod => _entity.DeliveryPeriod;
    public decimal Amount => _entity.Amount;
    public EnglishAndMathsInstalmentType Type => Enum.Parse<EnglishAndMathsInstalmentType>(_entity.Type);

    internal EnglishAndMathsInstalment(EnglishAndMathsInstalmentEntity entity)
    {
        _entity = entity;
    }

    public EnglishAndMathsInstalment(short academicYear, byte deliveryPeriod, decimal amount, EnglishAndMathsInstalmentType type)
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

    public EnglishAndMathsInstalmentEntity GetEntity()
    {
        return _entity;
    }

    public static EnglishAndMathsInstalment Get(EnglishAndMathsInstalmentEntity model)
    {
        return new EnglishAndMathsInstalment(model);
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