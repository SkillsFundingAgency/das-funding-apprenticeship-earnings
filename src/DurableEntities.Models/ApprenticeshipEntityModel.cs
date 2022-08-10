using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntityModel
    {
        [JsonProperty] public Guid ApprenticeshipKey { get; set; }
        [JsonProperty] public long ApprovalsApprenticeshipId { get; set; }

        [JsonProperty] public string Uln { get; set; }
        [JsonProperty] public long UKPRN { get; set; }
        [JsonProperty] public long EmployerAccountId { get; set; }
        [JsonProperty] public string LegalEntityName { get; set; }
        [JsonProperty] public DateTime ActualStartDate { get; set; }
        [JsonProperty] public DateTime PlannedEndDate { get; set; }
        [JsonProperty] public decimal AgreedPrice { get; set; }
        [JsonProperty] public string TrainingCode { get; set; }
        [JsonProperty] public long? FundingEmployerAccountId { get; set; }
        [JsonProperty] public FundingType FundingType { get; set; }

        [JsonProperty] public EarningsProfileEntityModel EarningsProfile { get; set; }
    }
}
