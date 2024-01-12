using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EarningsProfileEntityModel
    {
        [JsonProperty] public Guid EarningsProfileId { get; set; }
        [JsonProperty] public decimal AdjustedPrice { get; set; }
        [JsonProperty] public List<InstalmentEntityModel> Instalments { get; set; } = null!;
        [JsonProperty] public decimal CompletionPayment { get; set; }
    }
}
