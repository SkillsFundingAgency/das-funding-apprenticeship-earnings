using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCompletionCommand;

public class SaveCompletionCommandHandler : ICommandHandler<SaveCompletionCommand>
{
    private readonly ILogger<SaveCompletionCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _earningsRecalculatedEventBuilder;
    private readonly ISystemClockService _systemClock;

    public SaveCompletionCommandHandler(
        ILogger<SaveCompletionCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        IMessageSession messageSession,
        IApprenticeshipEarningsRecalculatedEventBuilder earningsRecalculatedEventBuilder,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _messageSession = messageSession;
        _earningsRecalculatedEventBuilder = earningsRecalculatedEventBuilder;
        _systemClock = systemClock;
    }

    public async Task Handle(SaveCompletionCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveCompletionCommand for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);

        var apprenticeship = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeship.UpdateCompletion(command.CompletionDetails.CompletionDate, _systemClock);
        apprenticeship.CalculateEarnings(_systemClock);

        await _apprenticeshipRepository.Update(apprenticeship);

        _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
        await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeship), cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully handled SaveCompletionCommand for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
    }
}