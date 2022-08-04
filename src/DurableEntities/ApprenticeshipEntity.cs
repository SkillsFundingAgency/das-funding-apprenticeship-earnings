using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntity
    {
        [JsonProperty] public ApprenticeshipEntityModel Model { get; set; }

        private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;

        public ApprenticeshipEntity(ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler)
        {
            _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
        }

        public async Task HandleApprenticeshipLearnerEvent(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            MapApprenticeshipLearnerEventProperties(apprenticeshipCreatedEvent);
            var apprenticeship = await _createApprenticeshipCommandHandler.Create(new CreateApprenticeshipCommand(Model));
        }

        [FunctionName(nameof(ApprenticeshipEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<ApprenticeshipEntity>();

        private void MapApprenticeshipLearnerEventProperties(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            Model = new ApprenticeshipEntityModel
            {
                ApprenticeshipKey = apprenticeshipCreatedEvent.ApprenticeshipKey,
                Uln = apprenticeshipCreatedEvent.Uln,
                UKPRN = apprenticeshipCreatedEvent.UKPRN,
                EmployerAccountId = apprenticeshipCreatedEvent.EmployerAccountId,
                ActualStartDate = apprenticeshipCreatedEvent.ActualStartDate,
                PlannedEndDate = apprenticeshipCreatedEvent.PlannedEndDate,
                AgreedPrice = apprenticeshipCreatedEvent.AgreedPrice,
                TrainingCode = apprenticeshipCreatedEvent.TrainingCode,
                FundingEmployerAccountId = apprenticeshipCreatedEvent.FundingEmployerAccountId,
                FundingType = apprenticeshipCreatedEvent.FundingType,
                ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
                LegalEntityName = apprenticeshipCreatedEvent.LegalEntityName
            };
        }
    }
}
