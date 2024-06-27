using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

public class ApprenticeshipEpisodeModel
{

    //ApprenticeshipEpisodeID
    [JsonProperty] public long UKPRN { get; set; }
    [JsonProperty] public long EmployerAccountId { get; set; }
    //LegalEntityName
    //TrainingCode
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
