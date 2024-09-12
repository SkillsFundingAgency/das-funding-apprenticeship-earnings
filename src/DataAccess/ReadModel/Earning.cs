using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Earning", Schema = "Query")]
    public class Earning
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public string Uln { get; set; } = null!;
        public long UKPRN { get; set; }
        public long EmployerAccountId { get; set; }
        public long? FundingEmployerAccountId { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public FundingType FundingType { get; set; }
        public short AcademicYear { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
        public Guid ApprenticeshipEpisodeKey { get; set; }
    }
}
