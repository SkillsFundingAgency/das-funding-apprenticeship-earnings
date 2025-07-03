using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

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
        _logger.LogInformation("Handling SaveCareDetailsCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);
        apprenticeshipDomainModel.UpdateCareDetails(command.HasEHCP, command.IsCareLeaver, command.CareLeaverEmployerConsentGiven, _systemClockService);

        var hasRecalculatedEarnings = apprenticeshipDomainModel.HasEvent<ArchiveEarningsProfileEvent>();

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        if (hasRecalculatedEarnings)
        {
            _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
            await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel));
        }
        else
        {
            _logger.LogInformation("No EarningsRecalculatedEvent to publish for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
        }
       
        _logger.LogInformation("Successfully handled SaveCareDetailsCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
    }

    private async Task<Domain.Apprenticeship.Apprenticeship> GetDomainApprenticeship(Guid apprenticeshipKey)
    {
        try
        {
            return await _apprenticeshipRepository.Get(apprenticeshipKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting apprenticeship {apprenticeshipKey} from repository", apprenticeshipKey);
            throw;
        }
    }
}
