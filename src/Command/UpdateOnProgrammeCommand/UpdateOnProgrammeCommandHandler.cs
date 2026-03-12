using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeCommandHandler : ICommandHandler<UpdateOnProgrammeCommand>
{
    private readonly ILogger<UpdateOnProgrammeCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateOnProgrammeCommandHandler(
        ILogger<UpdateOnProgrammeCommandHandler> logger,
        ILearningRepository learningRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _learningRepository = learningRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateOnProgrammeCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateOnProgrammeCommand for LearningKey: {LearningKey}", command.LearningKey);
        
        var learningDomainModel = await _learningRepository.GetApprenticeshipLearning(command.LearningKey);

        if(learningDomainModel == null)
            throw new InvalidOperationException($"Learning domain model not found for LearningKey: {command.LearningKey}");


        var episode = learningDomainModel.GetEpisode(command.Request.ApprenticeshipEpisodeKey);
        var request = command.Request;

        ExecuteAndLog(() => learningDomainModel.UpdateDateOfBirth(request.DateOfBirth), "update DateOfBirth");
        ExecuteAndLog(() => episode.UpdatePause(request.PauseDate), "update Pause");
        ExecuteAndLog(() => episode.UpdatePeriodsInLearning(request.ToEpisodePeriodsInLearning()), "update Periods in learning");
        ExecuteAndLog(() => episode.UpdateCompletion(request.CompletionDate), "update Completion");
        ExecuteAndLog(() => episode.UpdateWithdrawalDate(request.WithdrawalDate, _systemClock), "update Withdrawal");

        if (request.IncludesFundingBandMaximumUpdate)
        {
            ExecuteAndLog(() => episode.UpdateFundingBandMaximum(request.FundingBandMaximum!.Value), "update FundingBandMaximum");
            ExecuteAndLog(() => episode.UpdatePrices(request.Prices), "update Prices");
            episode.UpdateAgeAtStart(learningDomainModel.DateOfBirth);
        }

        ExecuteAndLog(() => learningDomainModel.UpdateCareDetails(request.Care.HasEHCP, request.Care.IsCareLeaver, request.Care.CareLeaverEmployerConsentGiven, _systemClock), "update Care Details");

        ExecuteAndLog(() => learningDomainModel.Calculate(_systemClock, JsonSerializer.Serialize(command.Request), request.ApprenticeshipEpisodeKey), "calculation onprogramme earnings");

        _logger.LogInformation("Updating LearningKey: {LearningKey} in repository", command.LearningKey);
        await _learningRepository.Update(learningDomainModel);

        _logger.LogInformation("Completed handling UpdateOnProgrammeCommand for LearningKey: {LearningKey}", command.LearningKey);
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