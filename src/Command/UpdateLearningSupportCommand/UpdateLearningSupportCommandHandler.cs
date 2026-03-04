using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportCommandHandler : ICommandHandler<UpdateLearningSupportCommand>
{
    private readonly ILogger<UpdateLearningSupportCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;
    private readonly ISystemClockService _systemClockService;

    public UpdateLearningSupportCommandHandler(
        ILogger<UpdateLearningSupportCommandHandler> logger,
        ILearningRepository learningRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _learningRepository = learningRepository;
        _systemClockService = systemClock;
    }

    public async Task Handle(UpdateLearningSupportCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateLearningSupportCommand for apprenticeship {LearningKey}", command.LearningKey);

        var learningSupportPayments = command.LearningSupportPayments.SelectMany(x=> 
            LearningSupportPayments.GenerateLearningSupportPayments(x.StartDate, x.EndDate))
            .DistinctBy(x => new { x.AcademicYear, x.DeliveryPeriod, x.DueDate })
            .ToList();

        var learningDomainModel = await GetDomainApprenticeship(command.LearningKey);

        learningDomainModel.AddAdditionalEarnings(learningSupportPayments, InstalmentTypes.LearningSupport, _systemClockService);

        await _learningRepository.Update(learningDomainModel);

        _logger.LogInformation("Successfully handled UpdateLearningSupportCommand for apprenticeship {LearningKey}", command.LearningKey);
    }

    private async Task<Domain.Models.Learning> GetDomainApprenticeship(Guid LearningKey)
    {
        try
        {
            return await _learningRepository.Get(LearningKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting apprenticeship {LearningKey} from repository", LearningKey);
            throw;
        }
    }
}
