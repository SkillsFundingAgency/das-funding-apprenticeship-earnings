using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

[Dapper.Contrib.Extensions.Table("Domain.EnglishAndMathsAdditionalPayment")]
[Table("EnglishAndMathsAdditionalPayment", Schema = "Domain")]
public class EnglishAndMathsAdditionalPaymentEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EnglishAndMathsKey { get; set; }

    public short AcademicYear { get; set; }

    public byte DeliveryPeriod { get; set; }

    [Precision(15, 5)]
    public decimal Amount { get; set; }

    public string AdditionalPaymentType { get; set; } = null!;

    public DateTime DueDate { get; set; }
}
