using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.History
{
    [Dapper.Contrib.Extensions.Table("History.EarningsProfileHistory")]
    [Table("EarningsProfileHistory", Schema = "History")]
    public class EarningsProfileHistory
    {
        [System.ComponentModel.DataAnnotations.Key]
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

}
