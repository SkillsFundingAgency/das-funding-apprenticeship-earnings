using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntityModel
    {
        [JsonProperty] public Guid ApprenticeshipKey { get; set; }
        [JsonProperty] public long ApprovalsApprenticeshipId { get; set; }
        [JsonProperty] public string Uln { get; set; } = null!;
        [JsonProperty] public string LegalEntityName { get; set; } = null!;
        [JsonProperty] public long? FundingEmployerAccountId { get; set; }
        [JsonProperty] public EarningsProfileEntityModel EarningsProfile { get; set; } = null!;
        [JsonProperty] public List<HistoryRecord<EarningsProfileEntityModel>> EarningsProfileHistory { get; set; } = null!;
        [JsonProperty] public List<ApprenticeshipEpisodeModel> ApprenticeshipEpisodes { get; set; } = null!;
        [JsonProperty] public int AgeAtStartOfApprenticeship { get; set; }
    }

    public class HistoryRecord<T> where T : class
    {
        public T Record { get; set; } = null!;
        public DateTime SupersededDate { get; set; }
    }
}
