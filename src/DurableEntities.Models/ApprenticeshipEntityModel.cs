using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

[JsonObject(MemberSerialization.OptIn)]
public class ApprenticeshipEntityModel
{
    [JsonProperty] public Guid ApprenticeshipKey { get; set; }
    [JsonProperty] public long ApprovalsApprenticeshipId { get; set; }
    [JsonProperty] public string Uln { get; set; } = null!;
    [JsonProperty] public List<ApprenticeshipEpisodeModel> ApprenticeshipEpisodes { get; set; } = null!;
}

public class HistoryRecord<T> where T : class
{
    public T Record { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}