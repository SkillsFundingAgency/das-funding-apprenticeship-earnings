using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public abstract class BaseInstalmentEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    [Precision(15, 5)]
    public decimal Amount { get; set; }
    public string Type { get; set; }

    public BaseInstalmentEntity(BaseInstalmentEntity original, Guid earningsProfileId)
    {
        Key = original.Key;
        EarningsProfileId = earningsProfileId;
        Amount = original.Amount;
        DeliveryPeriod = original.DeliveryPeriod;
        AcademicYear = original.AcademicYear;
        Type = original.Type;
    }

    public BaseInstalmentEntity() { }
}
