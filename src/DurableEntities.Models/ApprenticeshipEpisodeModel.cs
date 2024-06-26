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
    //EmployerAccountId
    //LegalEntityName
    //TrainingCode
    //FundingEmployerAccountId
    //FundingType
    //Prices[]
    //ActualStartDate
    //PlannedEndDate
    //AgreedPrice
    //FundingBandMaximum
    //EarningsProfile
    //EarningsProfileHistory[]

}
