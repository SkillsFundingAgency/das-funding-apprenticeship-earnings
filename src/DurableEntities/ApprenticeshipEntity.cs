using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntity
    {
        [JsonProperty] public Guid ApprenticeshipKey { get; set; }
        [JsonProperty] public long ApprovalsApprenticeshipId { get; set; }

        [JsonProperty] public long Uln { get; set; }
        [JsonProperty] public long UKPRN { get; set; }
        [JsonProperty] public long EmployerAccountId { get; set; }
        [JsonProperty] public string LegalEntityName { get; set; }
        [JsonProperty] public DateTime? ActualStartDate { get; set; }
        [JsonProperty] public DateTime? PlannedEndDate { get; set; }
        [JsonProperty] public decimal AgreedPrice { get; set; }
        [JsonProperty] public string TrainingCode { get; set; }
        [JsonProperty] public long? FundingEmployerAccountId { get; set; }
        [JsonProperty] public FundingType FundingType { get; set; }

        [JsonProperty] public EarningsProfile EarningsProfile { get; set; }

        private readonly IEarningsProfileGenerator _earningsProfileGenerator;

        public ApprenticeshipEntity(IEarningsProfileGenerator earningsProfileGenerator)
        {
            _earningsProfileGenerator = earningsProfileGenerator;
        }

        public async Task HandleApprenticeshipLearnerEvent(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            MapApprenticeshipLearnerEventProperties(apprenticeshipCreatedEvent);
            await _earningsProfileGenerator.GenerateEarnings(apprenticeshipCreatedEvent);
        }

        [FunctionName(nameof(ApprenticeshipEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ApprenticeshipEntity>();

        private void MapApprenticeshipLearnerEventProperties(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            ApprenticeshipKey = apprenticeshipCreatedEvent.ApprenticeshipKey;
            Uln = apprenticeshipCreatedEvent.Uln;
            UKPRN = apprenticeshipCreatedEvent.UKPRN;
            EmployerAccountId = apprenticeshipCreatedEvent.EmployerAccountId;
            ActualStartDate = apprenticeshipCreatedEvent.ActualStartDate;
            PlannedEndDate = apprenticeshipCreatedEvent.PlannedEndDate;
            AgreedPrice = apprenticeshipCreatedEvent.AgreedPrice;
            TrainingCode = apprenticeshipCreatedEvent.TrainingCode;
            FundingEmployerAccountId = apprenticeshipCreatedEvent.FundingEmployerAccountId;
            FundingType = apprenticeshipCreatedEvent.FundingType;
            ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId;
            LegalEntityName = apprenticeshipCreatedEvent.LegalEntityName;
        }
    }
}
