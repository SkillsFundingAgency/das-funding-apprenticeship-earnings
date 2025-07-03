using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEarningsQueryCommand;

public class UpdateEarningsQueryCommandHandler : ICommandHandler<UpdateEarningsQueryCommand>
{
    private ILogger<UpdateEarningsQueryCommandHandler> _logger;
    private readonly IEarningsQueryRepository _earningsRepository;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;

    public UpdateEarningsQueryCommandHandler(
        ILogger<UpdateEarningsQueryCommandHandler> logger, 
        IEarningsQueryRepository earningsRepository,
        IApprenticeshipRepository apprenticeshipRepository)
    {
        _logger = logger;
        _earningsRepository = earningsRepository;
        _apprenticeshipRepository = apprenticeshipRepository;
    }

    public async Task Handle(UpdateEarningsQueryCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateEarningsQueryCommand for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);

        var apprenticeship = await GetApprenticeship(command.ApprenticeshipKey);

        switch (command.UpdateType)
        {
            case UpdateEarningsQueryType.Create:
                _logger.LogInformation("Creating earnings query for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);

                await _earningsRepository.Add(apprenticeship);
                break;

            case UpdateEarningsQueryType.Recalculate:
                _logger.LogInformation("Recalculating earnings query for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);
                await _earningsRepository.Replace(apprenticeship);
                break;
        }

        _logger.LogInformation("Successfully handled UpdateEarningsQueryCommand for ApprenticeshipKey: {ApprenticeshipKey}", command.ApprenticeshipKey);
    }

    private async Task<Apprenticeship> GetApprenticeship(Guid apprenticeshipKey)
    {
        try
        {
            return await _apprenticeshipRepository.Get(apprenticeshipKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving apprenticeship with key: {ApprenticeshipKey}", apprenticeshipKey);
            throw;
        }
    }
}
