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

    public SaveCareDetailsCommandHandler(
        ILogger<SaveCareDetailsCommandHandler> logger, 
        IApprenticeshipRepository apprenticeshipRepository, 
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClockService = systemClock;
    }

    public async Task Handle(SaveCareDetailsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveCareDetailsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);
        apprenticeshipDomainModel.UpdateCareDetails(command.HasEHCP, command.IsCareLeaver, command.CareLeaverEmployerConsentGiven, _systemClockService);
        apprenticeshipDomainModel.Calculate(_systemClockService);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);
       
        _logger.LogInformation("Successfully handled SaveCareDetailsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
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
