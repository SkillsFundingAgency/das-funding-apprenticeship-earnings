using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InstalmentEntityModel
    {
        [JsonProperty] public short AcademicYear { get; set; }
        [JsonProperty] public byte DeliveryPeriod { get; set; }
        [JsonProperty] public decimal Amount { get; set; }
    }
}
