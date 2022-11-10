using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel
{
    [Table("Earning", Schema = "Query")]
    public class Earning
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public long UKPRN { get; set; }
        public long EmployerAccountId { get; set; }
        public long? FundingEmployerAccountId { get; set; }
        public FundingType FundingType { get; set; }
        public short AcademicYear { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
    }
}
