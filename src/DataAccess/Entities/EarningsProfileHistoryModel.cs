namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[Table("Domain.EarningsProfileHistory")]
[System.ComponentModel.DataAnnotations.Schema.Table("Domain.EarningsProfileHistory")]
public class EarningsProfileHistoryModel
{
    [Key]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public List<InstalmentHistoryModel> Instalments { get; set; } = null!;
    public decimal CompletionPayment { get; set; }
    public DateTime SupersededDate { get; set; }
}
