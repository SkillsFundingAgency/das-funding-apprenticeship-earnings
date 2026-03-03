using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class BaseAdditionalPaymentEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    [Precision(15, 5)]
    public decimal Amount { get; set; }
    public string AdditionalPaymentType { get; set; }
    public DateTime DueDate { get; set; }

    public BaseAdditionalPaymentEntity(BaseAdditionalPaymentEntity original, Guid earningsProfileId)
    {
        Key = original.Key;
        EarningsProfileId = earningsProfileId;
        Amount = original.Amount;
        DeliveryPeriod = original.DeliveryPeriod;
        AcademicYear = original.AcademicYear;
        AdditionalPaymentType = original.AdditionalPaymentType;
        DueDate = original.DueDate;
    }

    public BaseAdditionalPaymentEntity() { }
}

[Dapper.Contrib.Extensions.Table("Domain.AdditionalPaymentHistory")]
[Table("AdditionalPaymentHistory", Schema = "Domain")]
public class AdditionalPaymentHistoryEntity : BaseAdditionalPaymentEntity
{
    public Guid? OriginalKey { get; set; }
    public Guid? Version { get; set; }

    public AdditionalPaymentHistoryEntity(BaseAdditionalPaymentEntity original, Guid earningsProfileId, Guid version) : base(original, earningsProfileId) 
    {
        OriginalKey = original.Key;
        Key = Guid.NewGuid();
        Version = version;
    }

    public AdditionalPaymentHistoryEntity() { }
}