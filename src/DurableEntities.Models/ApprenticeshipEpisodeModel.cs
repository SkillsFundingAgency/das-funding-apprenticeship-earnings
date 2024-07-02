using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

public class ApprenticeshipEpisodeModel
{
    [JsonProperty] public Guid ApprenticeshipEpisodeKey { get; set; } //todo tech design says ID rather than Key but we can assume it will match the existing convention
    [JsonProperty] public long UKPRN { get; set; }
    [JsonProperty] public long EmployerAccountId { get; set; }
    [JsonProperty] public string LegalEntityName { get; set; } = null!;
    [JsonProperty] public string TrainingCode { get; set; } = null!;
    [JsonProperty] public long? FundingEmployerAccountId { get; set; }
    [JsonProperty] public FundingType FundingType { get; set; }
    [JsonProperty] public int AgeAtStartOfApprenticeship { get; set; }
    [JsonProperty] public List<PriceModel> Prices { get; set; }
    [JsonProperty] public EarningsProfileEntityModel EarningsProfile { get; set; }
    [JsonProperty] public List<HistoryRecord<EarningsProfileEntityModel>> EarningsProfileHistory { get; set; }
}

public class PriceModel
{
    [JsonProperty] public DateTime ActualStartDate { get; set; }
    [JsonProperty] public DateTime PlannedEndDate { get; set; }
    [JsonProperty] public decimal AgreedPrice { get; set; }
    [JsonProperty] public decimal FundingBandMaximum { get; set; }
}