using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;

public class SaveCareDetailsCommandHandler : ICommandHandler<SaveCareDetailsCommand>
{
    private readonly ILogger<SaveCareDetailsCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClockService;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _earningsRecalculatedEventBuilder;

    public SaveCareDetailsCommandHandler(
        ILogger<SaveCareDetailsCommandHandler> logger, 
        IApprenticeshipRepository apprenticeshipRepository, 
        ISystemClockService systemClock,
        IMessageSession messageSession,
        IApprenticeshipEarningsRecalculatedEventBuilder earningsRecalculatedEventBuilder)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClockService = systemClock;
        _messageSession = messageSession;
        _earningsRecalculatedEventBuilder = earningsRecalculatedEventBuilder;
    }

    public async Task Handle(SaveCareDetailsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveCareDetailsCommand for apprenticeship {LearningKey}", command.LearningKey);

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.LearningKey);
        apprenticeshipDomainModel.UpdateCareDetails(command.HasEHCP, command.IsCareLeaver, command.CareLeaverEmployerConsentGiven, _systemClockService);
        var hasRecalculatedEarnings = apprenticeshipDomainModel.HasEvent<EarningsRecalculatedEvent>();
        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        if (hasRecalculatedEarnings)
        {
            _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {LearningKey}", command.LearningKey);
            await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel));
        }
        else
        {
            _logger.LogInformation("No EarningsRecalculatedEvent to publish for apprenticeship {LearningKey}", command.LearningKey);
        }
       
        _logger.LogInformation("Successfully handled SaveCareDetailsCommand for apprenticeship {LearningKey}", command.LearningKey);
    }

    private async Task<Domain.Apprenticeship.Apprenticeship> GetDomainApprenticeship(Guid LearningKey)
    {
        try
        {
            return await _apprenticeshipRepository.Get(LearningKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting apprenticeship {LearningKey} from repository", LearningKey);
            throw;
        }
    }
}
