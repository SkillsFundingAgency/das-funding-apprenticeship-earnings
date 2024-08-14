using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

[JsonObject(MemberSerialization.OptIn)]
public class ApprenticeshipEntity
{
    [JsonProperty] public ApprenticeshipEntityModel Model { get; set; }

    private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IProcessEpisodeUpdatedCommandHandler _processEpisodeUpdatedCommandHandler;

    public ApprenticeshipEntity(
        ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler,
        IDomainEventDispatcher domainEventDispatcher,
        IProcessEpisodeUpdatedCommandHandler processEpisodeUpdatedCommandHandler)
    {
        _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
        _domainEventDispatcher = domainEventDispatcher;
        _processEpisodeUpdatedCommandHandler = processEpisodeUpdatedCommandHandler;
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
        var apprenticeship = await _processEpisodeUpdatedCommandHandler.RecalculateEarnings(new ProcessEpisodeUpdatedCommand(Model, startDateChangedEvent));
        UpdateEpisodes(apprenticeship);
            
        foreach (dynamic domainEvent in apprenticeship.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }

    public async Task HandleApprenticeshipPriceChangeApprovedEvent(ApprenticeshipPriceChangedEvent apprenticeshipPriceChangedEvent)
    {
        var apprenticeship = await _processEpisodeUpdatedCommandHandler.RecalculateEarnings(new ProcessEpisodeUpdatedCommand(Model, apprenticeshipPriceChangedEvent));

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
                ApprenticeshipEpisodeKey = apprenticeshipCreatedEvent.Episode.Key,
                UKPRN = apprenticeshipCreatedEvent.Episode.Ukprn,
                EmployerAccountId = apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                TrainingCode = apprenticeshipCreatedEvent.Episode.TrainingCode,
                FundingType = Enum.Parse<FundingType>(apprenticeshipCreatedEvent.Episode.FundingType.ToString()),
                LegalEntityName = apprenticeshipCreatedEvent.Episode.LegalEntityName,
                AgeAtStartOfApprenticeship = apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship,
                FundingEmployerAccountId = apprenticeshipCreatedEvent.Episode.FundingEmployerAccountId,
                Prices = apprenticeshipCreatedEvent.Episode.Prices.Select(x =>
                    new PriceModel
                    {
                        PriceKey = x.Key,
                        ActualStartDate = x.StartDate,
                        PlannedEndDate = x.EndDate,
                        FundingBandMaximum = x.FundingBandMaximum,
                        AgreedPrice = x.TotalPrice
                    }).ToList()
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
            Prices = x.Prices == null ? new List<PriceModel>() : x.Prices.Select(MapPricesToModel).ToList(),
            FundingType = x.FundingType,
            FundingEmployerAccountId = x.FundingEmployerAccountId,
            LegalEntityName = x.LegalEntityName,
            TrainingCode = x.TrainingCode
        }).ToList();
    }
}