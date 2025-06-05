using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Dapper.Contrib.Extensions.Table("Domain.MathsAndEnglishInstalment")]
[Table("MathsAndEnglishInstalment", Schema = "Domain")]
public class MathsAndEnglishInstalmentModel
{
    public MathsAndEnglishInstalmentModel() { }

    public MathsAndEnglishInstalmentModel(Guid mathsAndEnglishKey, short academicYear, byte deliveryPeriod, decimal amount)
    {
        Key = Guid.NewGuid();
        MathsAndEnglishKey = mathsAndEnglishKey;
        AcademicYear = academicYear;
        DeliveryPeriod = deliveryPeriod;
        Amount = amount;
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid MathsAndEnglishKey { get; set; }

    public short AcademicYear { get; set; }

    public byte DeliveryPeriod { get; set; }

    public decimal Amount { get; set; }
}