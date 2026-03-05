using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;

[Dapper.Contrib.Extensions.Table("Domain.EnglishAndMathsInstalment")]
[Table("EnglishAndMathsInstalment", Schema = "Domain")]
public class EnglishAndMathsInstalmentEntity
{
    public EnglishAndMathsInstalmentEntity() { }

    public EnglishAndMathsInstalmentEntity(Guid mathsAndEnglishKey, short academicYear, byte deliveryPeriod, decimal amount, string type)
    {
        Key = Guid.NewGuid();
        EnglishAndMathsKey = mathsAndEnglishKey;
        AcademicYear = academicYear;
        DeliveryPeriod = deliveryPeriod;
        Amount = amount;
        Type = type;
    }

    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EnglishAndMathsKey { get; set; }

    public short AcademicYear { get; set; }

    public byte DeliveryPeriod { get; set; }

    public decimal Amount { get; set; }
    public string Type { get; set; }
}