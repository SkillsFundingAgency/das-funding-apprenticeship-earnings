using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Application;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
using SFA.DAS.NServiceBus.Services;

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

        [JsonProperty] public EarningsProfile EarningsProfile { get; set; }

        private readonly IAdjustedPriceProcessor _adjustedPriceProcessor;
        private readonly IInstallmentsGenerator _installmentsGenerator;
        private readonly IMessageSession _messageSession;

        public ApprenticeshipEntity(IAdjustedPriceProcessor adjustedPriceProcessor,
            IInstallmentsGenerator installmentsGenerator, IMessageSession messageSession)
        {
            _adjustedPriceProcessor = adjustedPriceProcessor;
            _installmentsGenerator = installmentsGenerator;
            _messageSession = messageSession;
        }

        public async Task HandleApprenticeshipLearnerEvent(InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent)
        {
            //todo logging
            MapApprenticeshipLearnerEventProperties(apprenticeshipLearnerEvent);

            EarningsProfile = new EarningsProfile { AdjustedPrice = _adjustedPriceProcessor.CalculateAdjustedPrice(AgreedPrice) };
            EarningsProfile.Installments = _installmentsGenerator.Generate(EarningsProfile.AdjustedPrice.Value, ActualStartDate, PlannedEndDate);

            await _messageSession.Publish(new EarningsGeneratedEvent
            {
                ApprenticeshipKey = ApprenticeshipKey,
                CommitmentId = CommitmentId
            });
        }

        [FunctionName(nameof(ApprenticeshipEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ApprenticeshipEntity>();

        private void MapApprenticeshipLearnerEventProperties(InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent)
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
