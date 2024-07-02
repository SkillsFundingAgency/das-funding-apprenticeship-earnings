using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

#pragma warning disable CS8618
public class PriceModel
{
    [JsonProperty] public DateTime ActualStartDate { get; set; }
    [JsonProperty] public DateTime PlannedEndDate { get; set; }
    [JsonProperty] public decimal AgreedPrice { get; set; }
    [JsonProperty] public decimal FundingBandMaximum { get; set; }
}