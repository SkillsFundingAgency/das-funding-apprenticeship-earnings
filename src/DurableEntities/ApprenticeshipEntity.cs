using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApprenticeshipEntity
    {
        [JsonProperty] public ApprenticeshipEntityModel Model { get; set; }

        private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ApprenticeshipEntity(ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler, IDomainEventDispatcher domainEventDispatcher)
        {
            _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task HandleApprenticeshipLearnerEvent(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            MapApprenticeshipLearnerEventProperties(apprenticeshipCreatedEvent);
            var apprenticeship = await _createApprenticeshipCommandHandler.Handle(new CreateApprenticeshipCommand(Model));
            Model.EarningsProfile = MapEarningsProfileToModel(apprenticeship.EarningsProfile);
            foreach (dynamic domainEvent in apprenticeship.FlushEvents())
            {
                await _domainEventDispatcher.Send(domainEvent);
            }
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
                ActualStartDate = apprenticeshipCreatedEvent.ActualStartDate.Value,
                PlannedEndDate = apprenticeshipCreatedEvent.PlannedEndDate.Value,
                AgreedPrice = apprenticeshipCreatedEvent.AgreedPrice,
                TrainingCode = apprenticeshipCreatedEvent.TrainingCode,
                FundingEmployerAccountId = apprenticeshipCreatedEvent.FundingEmployerAccountId,
                FundingType = apprenticeshipCreatedEvent.FundingType,
                ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
                LegalEntityName = apprenticeshipCreatedEvent.LegalEntityName,
                FundingBandMaximum = apprenticeshipCreatedEvent.FundingBandMaximum,
                AgeAtStartOfApprenticeship = apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault() //todo when the story for filtering out non-pilot apprenticeships is done this should always have a value at this point
            };
        }

        private EarningsProfileEntityModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
        {
            return new EarningsProfileEntityModel
            {
                AdjustedPrice = earningsProfile.OnProgramTotal,
                CompletionPayment = earningsProfile.CompletionPayment,
                Instalments = MapInstalmentsToModel(earningsProfile.Instalments),
            };
        }

        private List<InstalmentEntityModel> MapInstalmentsToModel(List<Instalment> instalments)
        {
            return instalments.Select(x => new InstalmentEntityModel
            {
                AcademicYear = x.AcademicYear,
                DeliveryPeriod = x.DeliveryPeriod,
                Amount = x.Amount
            }).ToList();
        }
    }
}
