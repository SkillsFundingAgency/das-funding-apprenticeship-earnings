namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class EarningsProfileModelBase
{
    [Key]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }

    public EarningsProfileModelBase() { }

    public EarningsProfileModelBase(EarningsProfileModelBase original)
    {
        EarningsProfileId = original.EarningsProfileId;
        EpisodeKey = original.EpisodeKey;
        CompletionPayment = original.CompletionPayment;
        OnProgramTotal = original.OnProgramTotal;
    }
}

[Table("Domain.EarningsProfile")]
[System.ComponentModel.DataAnnotations.Schema.Table("EarningsProfile", Schema = "Domain")]
public class EarningsProfileModel : EarningsProfileModelBase
{
    public List<InstalmentModel> Instalments { get; set; } = null!;
}

[Table("Domain.EarningsProfileHistory")]
[System.ComponentModel.DataAnnotations.Schema.Table("EarningsProfileHistory", Schema = "Domain")]
public class EarningsProfileHistoryModel : EarningsProfileModelBase
{
    public EarningsProfileHistoryModel(EarningsProfileModel original, DateTime supersededDate) : base(original)
    {
        SupersededDate = supersededDate;
        Instalments = original.Instalments.Select(x => new InstalmentHistoryModel(x, EarningsProfileId)).ToList();
    }

    public EarningsProfileHistoryModel() {}

    public List<InstalmentHistoryModel> Instalments { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}