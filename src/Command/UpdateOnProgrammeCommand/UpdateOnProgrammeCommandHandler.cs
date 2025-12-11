using Azure.Core;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeCommandHandler : ICommandHandler<UpdateOnProgrammeCommand>
{
    private readonly ILogger<UpdateOnProgrammeCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateOnProgrammeCommandHandler(
        ILogger<UpdateOnProgrammeCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateOnProgrammeCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateOnProgrammeCommand for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);
        
        var apprenticeship = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);
        var episode = apprenticeship.GetEpisode(command.Request.ApprenticeshipEpisodeKey);
        var request = command.Request;

        ExecuteAndLog(() => apprenticeship.UpdateDateOfBirth(request.DateOfBirth), "update DateOfBirth");
        ExecuteAndLog(() => apprenticeship.Pause(request.PauseDate, _systemClock), "update Pause");
        ExecuteAndLog(() => episode.UpdateBreaksInLearning(request.ToEpisodeBreaksInLearning()), "update Breaks in learning");

        if (request.IncludesFundingBandMaximumUpdate)
        {
            ExecuteAndLog(() => apprenticeship.UpdatePrices(request.Prices, request.ApprenticeshipEpisodeKey, request.FundingBandMaximum!.Value, _systemClock), "update Prices");
        }

        ExecuteAndLog(() => apprenticeship.Calculate(_systemClock, request.ApprenticeshipEpisodeKey), "calculation onprogramme earnings");

        _logger.LogInformation("Updating ApprenticeshipKey: {ApprenticeshipKey} in repository", command.ApprenticeshipKey);
        await _apprenticeshipRepository.Update(apprenticeship);

        _logger.LogInformation("Completed handling UpdateOnProgrammeCommand for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);
    }



    private void ExecuteAndLog(Action action, string actionDescription)
    {
        _logger.LogInformation("Starting {action}", actionDescription);

        try
        {
            action();
            _logger.LogInformation("Completed {action}", actionDescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during {action}", actionDescription);
            throw;
        }
    }
}