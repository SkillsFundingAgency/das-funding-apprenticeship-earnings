namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public class InstalmentModelBase
{
    [Key]
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }

    public InstalmentModelBase(InstalmentModelBase original)
    {
        Key = original.Key;
        EarningsProfileId = original.EarningsProfileId;
        Amount = original.Amount;
        DeliveryPeriod = original.DeliveryPeriod;
        AcademicYear = original.AcademicYear;
    }

    public InstalmentModelBase() { }
}

[Table("Domain.Instalment")]
[System.ComponentModel.DataAnnotations.Schema.Table("Instalment", Schema = "Domain")]
public class InstalmentModel : InstalmentModelBase
{

}

[Table("Domain.InstalmentHistory")]
[System.ComponentModel.DataAnnotations.Schema.Table("InstalmentHistory", Schema = "Domain")]
public class InstalmentHistoryModel : InstalmentModelBase
{
    public InstalmentHistoryModel(InstalmentModelBase original) : base(original) { }
    public InstalmentHistoryModel() { }
}