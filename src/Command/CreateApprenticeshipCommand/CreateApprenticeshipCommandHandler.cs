using Microsoft.Extensions.Internal;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using ApprenticeshipEpisode = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.ApprenticeshipEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;

public class CreateApprenticeshipCommandHandler : ICreateApprenticeshipCommandHandler
{
    private readonly IApprenticeshipFactory _apprenticeshipFactory;
    private readonly IMessageSession _messageSession;
    private readonly IEarningsGeneratedEventBuilder _earningsGeneratedEventBuilder; 
    private readonly ISystemClockService _systemClock;

    public CreateApprenticeshipCommandHandler(
        IApprenticeshipFactory apprenticeshipFactory, IMessageSession messageSession, IEarningsGeneratedEventBuilder earningsGeneratedEventBuilder, ISystemClockService systemClock)
    {
        _apprenticeshipFactory = apprenticeshipFactory;
        _messageSession = messageSession;
        _earningsGeneratedEventBuilder = earningsGeneratedEventBuilder;
        _systemClock = systemClock;
    }

    public async Task<Apprenticeship> Create(CreateApprenticeshipCommand command)
    {
        var apprenticeship = CreateApprenticeshipModel(command.ApprenticeshipCreatedEvent);
        apprenticeship.CalculateEarnings(_systemClock);
        await _messageSession.Publish(_earningsGeneratedEventBuilder.Build(apprenticeship));
        return apprenticeship;
    }

    private Apprenticeship CreateApprenticeshipModel(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        var apprenticeshipEpisodes = new List<ApprenticeshipEpisode> { new ApprenticeshipEpisode
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
            } 
        };

        return _apprenticeshipFactory.CreateNew(apprenticeshipCreatedEvent.ApprovalsApprenticeshipId, apprenticeshipCreatedEvent.Uln, apprenticeshipEpisodes);
    }
}
