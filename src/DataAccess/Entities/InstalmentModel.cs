namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.Instalment")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.Instalment")]
public class InstalmentModel
{
    [Key]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}
