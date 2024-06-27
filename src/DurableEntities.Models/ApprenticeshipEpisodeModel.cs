using Newtonsoft.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

public class ApprenticeshipEpisodeModel
{

    //ApprenticeshipEpisodeID
    [JsonProperty] public long UKPRN { get; set; }
    [JsonProperty] public long EmployerAccountId { get; set; }
    //LegalEntityName
    [JsonProperty] public string TrainingCode { get; set; } = null!;
    //FundingEmployerAccountId
    //FundingType
    //Prices[]
    [JsonProperty] public DateTime ActualStartDate { get; set; }
    [JsonProperty] public DateTime PlannedEndDate { get; set; }
    [JsonProperty] public decimal AgreedPrice { get; set; }
    //FundingBandMaximum
    //EarningsProfile
    //EarningsProfileHistory[]

}
