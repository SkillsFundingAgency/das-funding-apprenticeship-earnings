using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public enum InstalmentType
{
    Regular = 0,
    Completion = 1,
    Balancing = 2
}

public abstract class BaseInstalment<TEntity> : IDomainEntity<TEntity> where TEntity : BaseInstalmentEntity, new()
{
    protected TEntity _entity;

    public short AcademicYear => _entity.AcademicYear;
    public byte DeliveryPeriod => _entity.DeliveryPeriod;
    public decimal Amount => _entity.Amount;
    public InstalmentType Type => Enum.Parse<InstalmentType>(_entity.Type);

    public BaseInstalment(short academicYear, byte deliveryPeriod, decimal amount, InstalmentType instalmentType = InstalmentType.Regular)
    {
        _entity = new TEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            DeliveryPeriod = deliveryPeriod,
            Amount = amount,
            Type = instalmentType.ToString()
        };
    }

    protected BaseInstalment(TEntity model)
    {
        _entity = model;
    }

    public TEntity GetEntity()
    {
        return _entity;
    }

    public bool AreSame(TEntity? compare)
    {
        if (compare == null)
            return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount;
    }
}
