using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;

public class SaveLearningSupportCommandHandler : ICommandHandler<SaveLearningSupportCommand>
{
    private readonly ILogger<SaveLearningSupportCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClockService;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _earningsRecalculatedEventBuilder;

    public SaveLearningSupportCommandHandler(
        ILogger<SaveLearningSupportCommandHandler> logger,
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

    public async Task Handle(SaveLearningSupportCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveLearningSupportCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);

        _logger.LogInformation("Generating learning support payments");
        var stopwatch = Stopwatch.StartNew();

        var learningSupportPayments = command.LearningSupportPayments.SelectMany(x=> 
        LearningSupportPayments.GenerateLearningSupportPayments(x.StartDate, x.EndDate)).ToList();

        _logger.LogInformation($"Done generating learning support payments, elapsed: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Restart();
        
        _logger.LogInformation("Getting apprenticeship domain object");
        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);
        _logger.LogInformation($"Done getting apprenticeship domain object, elapsed: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Restart();

        _logger.LogInformation("Adding additional earnings");
        apprenticeshipDomainModel.AddAdditionalEarnings(learningSupportPayments, InstalmentTypes.LearningSupport, _systemClockService);
        _logger.LogInformation($"Done adding additional earnings, elapsed: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Restart();

        _logger.LogInformation("Updating apprenticeship in repository");
        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
        _logger.LogInformation($"Done updating apprenticeship in repository, elapsed: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Restart();

        _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
        await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel), cancellationToken);

        _logger.LogInformation($"Done publishing EarningsRecalculatedEvent, elapsed: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.Stop();

        _logger.LogInformation("Successfully handled SaveLearningSupportCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
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
