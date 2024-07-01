using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

public class ApprenticeshipEpisodeModel
{

    //ApprenticeshipEpisodeID
    [JsonProperty] public long UKPRN { get; set; }
    [JsonProperty] public long EmployerAccountId { get; set; }
    [JsonProperty] public string LegalEntityName { get; set; } = null!;
    [JsonProperty] public string TrainingCode { get; set; } = null!;
    [JsonProperty] public long? FundingEmployerAccountId { get; set; }
    [JsonProperty] public FundingType FundingType { get; set; }
    //Prices[]
    [JsonProperty] public DateTime ActualStartDate { get; set; }
    [JsonProperty] public DateTime PlannedEndDate { get; set; }
    [JsonProperty] public int AgeAtStartOfApprenticeship { get; set; }
    [JsonProperty] public decimal AgreedPrice { get; set; }
    [JsonProperty] public decimal FundingBandMaximum { get; set; }
    [JsonProperty] public EarningsProfileEntityModel EarningsProfile { get; set; } = null!;
    [JsonProperty] public List<HistoryRecord<EarningsProfileEntityModel>> EarningsProfileHistory { get; set; } = null!;

}
