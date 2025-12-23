using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportCommandHandler : ICommandHandler<UpdateLearningSupportCommand>
{
    private readonly ILogger<UpdateLearningSupportCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClockService;

    public UpdateLearningSupportCommandHandler(
        ILogger<UpdateLearningSupportCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClockService = systemClock;
    }

    public async Task Handle(UpdateLearningSupportCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateLearningSupportCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);

        var learningSupportPayments = command.LearningSupportPayments.SelectMany(x=> 
            LearningSupportPayments.GenerateLearningSupportPayments(x.StartDate, x.EndDate))
            .DistinctBy(x => new { x.AcademicYear, x.DeliveryPeriod, x.DueDate })
            .ToList();

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);

        apprenticeshipDomainModel.AddAdditionalEarnings(learningSupportPayments, InstalmentTypes.LearningSupport, _systemClockService);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Successfully handled UpdateLearningSupportCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
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
