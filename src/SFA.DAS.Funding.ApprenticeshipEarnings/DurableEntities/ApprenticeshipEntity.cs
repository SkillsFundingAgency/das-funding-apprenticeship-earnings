using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntity
    {
        [JsonProperty] public string ApprenticeshipKey { get; set; }
        [JsonProperty] public long CommitmentId { get; set; }
        [JsonProperty] public DateTime ApprovedOn { get; set; }
        [JsonProperty] public DateTime AgreedOn { get; set; }
        [JsonProperty] public long Uln { get; set; }
        [JsonProperty] public long ProviderId { get; set; }
        [JsonProperty] public long EmployerId { get; set; }
        [JsonProperty] public DateTime ActualStartDate { get; set; }
        [JsonProperty] public DateTime PlannedEndDate { get; set; }
        [JsonProperty] public decimal AgreedPrice { get; set; }
        [JsonProperty] public string TrainingCode { get; set; }
        [JsonProperty] public long? TransferSenderEmployerId { get; set; }
        [JsonProperty] public EmployerType EmployerType { get; set; }

        [JsonProperty] public decimal? AdjustedPrice { get; set; }


        private readonly IAdjustedPriceProcessor _adjustedPriceProcessor;

        public ApprenticeshipEntity(IAdjustedPriceProcessor adjustedPriceProcessor)
        {
            _adjustedPriceProcessor = adjustedPriceProcessor;
        }

        public async Task Process(InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent)
        {
            //todo logging
            MapApprenticeshipLearnerEventProperties(apprenticeshipLearnerEvent);
            AdjustedPrice = _adjustedPriceProcessor.CalculateAdjustedPrice(AgreedPrice);
        }

        [FunctionName(nameof(ApprenticeshipEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ApprenticeshipEntity>();

        private void MapApprenticeshipLearnerEventProperties( InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent)
        {
            ApprenticeshipKey = apprenticeshipLearnerEvent.ApprenticeshipKey;
            CommitmentId = apprenticeshipLearnerEvent.CommitmentId;
            ApprovedOn = apprenticeshipLearnerEvent.ApprovedOn;
            AgreedOn = apprenticeshipLearnerEvent.AgreedOn;
            Uln = apprenticeshipLearnerEvent.Uln;
            ProviderId = apprenticeshipLearnerEvent.ProviderId;
            EmployerId = apprenticeshipLearnerEvent.EmployerId;
            ActualStartDate = apprenticeshipLearnerEvent.ActualStartDate;
            PlannedEndDate = apprenticeshipLearnerEvent.PlannedEndDate;
            AgreedPrice = apprenticeshipLearnerEvent.AgreedPrice;
            TrainingCode = apprenticeshipLearnerEvent.TrainingCode;
            TransferSenderEmployerId = apprenticeshipLearnerEvent.TransferSenderEmployerId;
            EmployerType = apprenticeshipLearnerEvent.EmployerType;
        }
    }
}
