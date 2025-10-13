using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class InstalmentModelBase
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodePriceKey { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    [Precision(15, 5)]
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public bool IsAfterLearningEnded { get; set; }

    public InstalmentModelBase(InstalmentModelBase original, Guid earningsProfileId)
    {
        Key = original.Key;
        EarningsProfileId = earningsProfileId;
        Amount = original.Amount;
        DeliveryPeriod = original.DeliveryPeriod;
        AcademicYear = original.AcademicYear;
        Type = original.Type;
        IsAfterLearningEnded = original.IsAfterLearningEnded;
    }

    public InstalmentModelBase() { }
}

[Dapper.Contrib.Extensions.Table("Domain.Instalment")]
[Table("Instalment", Schema = "Domain")]
public class InstalmentModel : InstalmentModelBase
{

}
