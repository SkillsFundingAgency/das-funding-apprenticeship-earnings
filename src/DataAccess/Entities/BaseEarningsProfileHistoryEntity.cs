using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public abstract class BaseEarningsProfileHistoryEntity
{
    [Key]
    public Guid Key { get; set; }

    [Required]
    public Guid EarningsProfileId { get; set; }

    [Required]
    public Guid Version { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Required]
    public string State { get; set; } = string.Empty;
}
