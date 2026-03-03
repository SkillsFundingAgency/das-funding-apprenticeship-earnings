using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class AdditionalPayment : IDomainEntity<ApprenticeshipAdditionalPaymentEntity>
{
    private ApprenticeshipAdditionalPaymentEntity _model;

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public string AdditionalPaymentType => _model.AdditionalPaymentType;
    public DateTime DueDate => _model.DueDate;

    private AdditionalPayment(ApprenticeshipAdditionalPaymentEntity model)
    {
        _model = model;
    }

    public AdditionalPayment(short academicYear, byte deliveryPeriod, decimal amount, DateTime dueDate, string incentiveType)
    {
        _model = new ApprenticeshipAdditionalPaymentEntity
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            Amount = amount,
            DeliveryPeriod = deliveryPeriod,
            DueDate = dueDate,
            AdditionalPaymentType = incentiveType
        };
    }

    public ApprenticeshipAdditionalPaymentEntity GetModel()
    {
        return _model;
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