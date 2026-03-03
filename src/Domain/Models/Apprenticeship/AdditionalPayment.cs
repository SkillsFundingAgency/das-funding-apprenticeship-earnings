using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class AdditionalPayment : IDomainEntity<ApprenticeshipAdditionalPaymentEntity>
{
    private ApprenticeshipAdditionalPaymentEntity _entity;

    public short AcademicYear => _entity.AcademicYear;
    public byte DeliveryPeriod => _entity.DeliveryPeriod;
    public decimal Amount => _entity.Amount;
    public string AdditionalPaymentType => _entity.AdditionalPaymentType;
    public DateTime DueDate => _entity.DueDate;

    private AdditionalPayment(ApprenticeshipAdditionalPaymentEntity entity)
    {
        _entity = entity;
    }

    public AdditionalPayment(short academicYear, byte deliveryPeriod, decimal amount, DateTime dueDate, string incentiveType)
    {
        _entity = new ApprenticeshipAdditionalPaymentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            Amount = amount,
            DeliveryPeriod = deliveryPeriod,
            DueDate = dueDate,
            AdditionalPaymentType = incentiveType
        };
    }

    public ApprenticeshipAdditionalPaymentEntity GetEntity()
    {
        return _entity;
    }

    public static AdditionalPayment Get(ApprenticeshipAdditionalPaymentEntity model)
    {
        return new AdditionalPayment(model);
    }

    public bool AreSame(ApprenticeshipAdditionalPaymentEntity? compare)
    {
        if (compare == null)
            return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount &&
               AdditionalPaymentType == compare.AdditionalPaymentType &&
               DueDate == compare.DueDate;
    }
}