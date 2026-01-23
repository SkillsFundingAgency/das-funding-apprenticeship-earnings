using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

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
        ExecuteAndLog(() => episode.UpdatePause(request.PauseDate), "update Pause");
        ExecuteAndLog(() => episode.UpdatePeriodsInLearning(request.ToEpisodePeriodsInLearning()), "update Periods in learning");
        ExecuteAndLog(() => episode.UpdateCompletion(request.CompletionDate), "update Completion");
        ExecuteAndLog(() => episode.UpdateWithdrawalDate(request.WithdrawalDate, _systemClock), "update Withdrawal");

        if (request.IncludesFundingBandMaximumUpdate)
        {
            ExecuteAndLog(() => episode.UpdateFundingBandMaximum(request.FundingBandMaximum!.Value), "update FundingBandMaximum");
            ExecuteAndLog(() => episode.UpdatePrices(request.Prices), "update Prices");
        }

        ExecuteAndLog(() => apprenticeship.UpdateCareDetails(request.Care.HasEHCP, request.Care.IsCareLeaver, request.Care.CareLeaverEmployerConsentGiven, _systemClock), "update Care Details");

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