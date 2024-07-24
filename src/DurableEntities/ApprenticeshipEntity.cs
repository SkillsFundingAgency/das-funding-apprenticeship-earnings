using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

[JsonObject(MemberSerialization.OptIn)]
public class ApprenticeshipEntity
{
    [JsonProperty] public ApprenticeshipEntityModel Model { get; set; }

    private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;
    private readonly IApprovePriceChangeCommandHandler _approvePriceChangeCommandHandler;
    private readonly IApproveStartDateChangeCommandHandler _startDateChangeApprovedCommandHandler;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ApprenticeshipEntity(
        ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler,
        IApprovePriceChangeCommandHandler approvePriceChangeCommandHandler,
        IApproveStartDateChangeCommandHandler startDateChangeApprovedCommandHandler,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
        _approvePriceChangeCommandHandler = approvePriceChangeCommandHandler;
        _startDateChangeApprovedCommandHandler = startDateChangeApprovedCommandHandler;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task HandleApprenticeshipLearnerEvent(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        MapApprenticeshipLearnerEventProperties(apprenticeshipCreatedEvent);
        var apprenticeship = await _createApprenticeshipCommandHandler.Create(new CreateApprenticeshipCommand(Model));

        UpdateEpisodes(apprenticeship);

        foreach (dynamic domainEvent in apprenticeship.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task HandleApprenticeshipStartDateChangeApprovedEvent(ApprenticeshipStartDateChangedEvent startDateChangedEvent)
    {
        var apprenticeship = await _startDateChangeApprovedCommandHandler.RecalculateEarnings(new ApproveStartDateChangeCommand(Model, startDateChangedEvent));
        UpdateEpisodes(apprenticeship);
            
        foreach (dynamic domainEvent in apprenticeship.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task HandleApprenticeshipPriceChangeApprovedEvent(PriceChangeApprovedEvent priceChangeApprovedEvent)
    {
        var apprenticeship = await _approvePriceChangeCommandHandler.RecalculateEarnings(new ApprovePriceChangeCommand(Model, priceChangeApprovedEvent));

        UpdateEpisodes(apprenticeship);

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
            ApprovalsApprenticeshipId = apprenticeshipCreatedEvent.ApprovalsApprenticeshipId,
            ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeModel> { new ApprenticeshipEpisodeModel
            {
                ApprenticeshipEpisodeKey = apprenticeshipCreatedEvent.ApprenticeshipEpisodeKey,
                UKPRN = apprenticeshipCreatedEvent.UKPRN,
                EmployerAccountId = apprenticeshipCreatedEvent.EmployerAccountId,
                TrainingCode = apprenticeshipCreatedEvent.TrainingCode,
                FundingType = apprenticeshipCreatedEvent.FundingType,
                LegalEntityName = apprenticeshipCreatedEvent.LegalEntityName,
                AgeAtStartOfApprenticeship = apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault(), //todo when the story for filtering out non-pilot apprenticeships is done this should always have a value at this point
                FundingEmployerAccountId = apprenticeshipCreatedEvent.FundingEmployerAccountId,
                Prices = new List<PriceModel>
                {
                    new()
                    {
                        PriceKey = apprenticeshipCreatedEvent.PriceKey,
                        ActualStartDate = apprenticeshipCreatedEvent.ActualStartDate.Value,
                        PlannedEndDate = apprenticeshipCreatedEvent.PlannedEndDate.Value,
                        AgreedPrice = apprenticeshipCreatedEvent.AgreedPrice,
                        FundingBandMaximum = apprenticeshipCreatedEvent.FundingBandMaximum
                    }
                }
            }},
        };
    }

    private EarningsProfileEntityModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
    {
        return new EarningsProfileEntityModel
        {
            AdjustedPrice = earningsProfile.OnProgramTotal,
            CompletionPayment = earningsProfile.CompletionPayment,
            Instalments = MapInstalmentsToModel(earningsProfile.Instalments),
            EarningsProfileId = earningsProfile.EarningsProfileId
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

    private PriceModel MapPricesToModel(Price price)
    {
        return new PriceModel
        {
            PriceKey = price.PriceKey,
            FundingBandMaximum = price.FundingBandMaximum,
            ActualStartDate = price.ActualStartDate,
            AgreedPrice = price.AgreedPrice,
            PlannedEndDate = price.PlannedEndDate
        };
    }

    private void UpdateEpisodes(Apprenticeship apprenticeship)
    {
        Model.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeModel
        {
            ApprenticeshipEpisodeKey = x.ApprenticeshipEpisodeKey,
            UKPRN = x.UKPRN,
            EmployerAccountId = x.EmployerAccountId,
            AgeAtStartOfApprenticeship = x.AgeAtStartOfApprenticeship,
            EarningsProfile = MapEarningsProfileToModel(x.EarningsProfile),
            EarningsProfileHistory = x.EarningsProfileHistory.Select(ep => new Models.HistoryRecord<EarningsProfileEntityModel> 
            { 
                Record = MapEarningsProfileToModel(ep.Record),
                SupersededDate = ep.SupersededDate
            }).ToList(),
            Prices = x.Prices == null ? new List<PriceModel>() : x.Prices.Select(MapPricesToModel).ToList()
        }).ToList();
    }
}