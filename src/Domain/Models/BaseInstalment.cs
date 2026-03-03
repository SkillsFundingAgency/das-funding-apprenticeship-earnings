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
    protected TEntity _model;

    public BaseInstalment(short academicYear, byte deliveryPeriod, decimal amount, InstalmentType instalmentType = InstalmentType.Regular)
    {
        _model = new TEntity
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
        _model = model;
    }

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public InstalmentType Type => Enum.Parse<InstalmentType>(_model.Type);

    public TEntity GetModel()
    {
        return _model;
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
