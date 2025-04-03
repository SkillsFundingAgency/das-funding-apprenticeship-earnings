using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;

public class SaveCareDetailsCommandHandler : ICommandHandler<SaveCareDetailsCommand>
{
    private readonly ILogger<SaveCareDetailsCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;

    public SaveCareDetailsCommandHandler(ILogger<SaveCareDetailsCommandHandler> logger, IApprenticeshipRepository apprenticeshipRepository)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
    }

    public async Task Handle(SaveCareDetailsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveCareDetailsCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);
        apprenticeshipDomainModel.UpdateCareDetails(command.HasEHCP, command.IsCareLeaver, command.CareLeaverEmployerConsentGiven);
        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

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
