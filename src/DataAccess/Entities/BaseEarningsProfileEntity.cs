using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

[System.ComponentModel.DataAnnotations.Schema.NotMapped]
public abstract class BaseEarningsProfileEntity
{
    [Dapper.Contrib.Extensions.Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public Guid Version { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }
    public bool IsApproved { get; set; }
    public string CalculationData { get; set; }

    public BaseEarningsProfileEntity() { }

    public BaseEarningsProfileEntity(BaseEarningsProfileEntity original)
    {
        EarningsProfileId = Guid.NewGuid();
        EpisodeKey = original.EpisodeKey;
        CompletionPayment = original.CompletionPayment;
        OnProgramTotal = original.OnProgramTotal;
        Version = original.Version;
    }
}
