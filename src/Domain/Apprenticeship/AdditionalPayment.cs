using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class AdditionalPayment : IDomainEntity<AdditionalPaymentModel>
{
    private AdditionalPaymentModel _model;

    public short AcademicYear => _model.AcademicYear;
    public byte DeliveryPeriod => _model.DeliveryPeriod;
    public decimal Amount => _model.Amount;
    public string AdditionalPaymentType => _model.AdditionalPaymentType;
    public DateTime DueDate => _model.DueDate;
    public bool IsAfterLearningEnded => _model.IsAfterLearningEnded;

    private AdditionalPayment(AdditionalPaymentModel model)
    {
        _model = model;
    }

    public AdditionalPayment(short academicYear, byte deliveryPeriod, decimal amount, DateTime dueDate, string incentiveType, bool isAfterLearningEnded = false)
    {
        _model = new AdditionalPaymentModel
        {
            Key = Guid.NewGuid(),
            AcademicYear = academicYear,
            Amount = amount,
            DeliveryPeriod = deliveryPeriod,
            DueDate = dueDate,
            AdditionalPaymentType = incentiveType,
            IsAfterLearningEnded = isAfterLearningEnded
        };
    }

    public AdditionalPaymentModel GetModel()
    {
        return _model;
    }

    public static AdditionalPayment Get(AdditionalPaymentModel model)
    {
        return new AdditionalPayment(model);
    }

    internal void SoftDelete()
    {
        _model.IsAfterLearningEnded = true;
    }

    public bool AreSame(AdditionalPaymentModel? compare)
    {
        if (compare == null)
            return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount &&
               AdditionalPaymentType == compare.AdditionalPaymentType &&
               DueDate == compare.DueDate &&
               IsAfterLearningEnded == compare.IsAfterLearningEnded;
    }
}