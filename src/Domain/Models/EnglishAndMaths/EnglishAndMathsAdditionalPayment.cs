using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

public class EnglishAndMathsAdditionalPayment : IDomainEntity<EnglishAndMathsAdditionalPaymentEntity>
{
    private EnglishAndMathsAdditionalPaymentEntity _entity;

    public short AcademicYear => _entity.AcademicYear;
    public byte DeliveryPeriod => _entity.DeliveryPeriod;
    public decimal Amount => _entity.Amount;
    public string AdditionalPaymentType => _entity.AdditionalPaymentType;
    public DateTime DueDate => _entity.DueDate;

    private EnglishAndMathsAdditionalPayment(EnglishAndMathsAdditionalPaymentEntity entity)
    {
        _entity = entity;
    }

    public EnglishAndMathsAdditionalPayment(short academicYear, byte deliveryPeriod, decimal amount, DateTime dueDate, string additionalPaymentType)
    {
        _entity = new EnglishAndMathsAdditionalPaymentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            Amount = amount,
            DeliveryPeriod = deliveryPeriod,
            DueDate = dueDate,
            AdditionalPaymentType = additionalPaymentType
        };
    }

    public EnglishAndMathsAdditionalPaymentEntity GetEntity() => _entity;

    public static EnglishAndMathsAdditionalPayment Get(EnglishAndMathsAdditionalPaymentEntity model) => new EnglishAndMathsAdditionalPayment(model);

    public bool AreSame(EnglishAndMathsAdditionalPaymentEntity? compare)
    {
        if (compare == null) return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount &&
               AdditionalPaymentType == compare.AdditionalPaymentType &&
               DueDate == compare.DueDate;
    }
}
