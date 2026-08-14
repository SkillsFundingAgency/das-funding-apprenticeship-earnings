using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommandHandler : ICommandHandler<ApproveLearningCommand>
{
    private readonly ILearningDomainService _learningDomainService;
    private readonly ILogger<ApproveLearningCommandHandler> _logger;

    public ApproveLearningCommandHandler(ILearningDomainService learningDomainService, ILogger<ApproveLearningCommandHandler> logger)
    {
        _learningDomainService = learningDomainService;
        _logger = logger;
    }

    public async Task Handle(ApproveLearningCommand command, CancellationToken cancellationToken = default)
    {
        var learning = await _learningDomainService.GetLearning(command.LearningKey);

        if (learning == null)
        {
            _logger.LogInformation(
                "No draft Earnings found for LearningKey {LearningKey} on approval - expected when earnings generation is disabled",
                command.LearningKey);
            return;
        }

        if (learning is ShortCourseLearning shortCourseLearning)
        {
            shortCourseLearning.SetApprovalsApprenticeshipId(command.ApprovalsApprenticeshipId);
            shortCourseLearning.SetEmployerType(command.EpisodeKey, command.EmployerType);
        }
        else if (learning is ApprenticeshipLearning apprenticeshipLearning)
        {
            apprenticeshipLearning.SetApprovalsApprenticeshipId(command.ApprovalsApprenticeshipId);
            apprenticeshipLearning.SetEmployerType(command.EpisodeKey, command.EmployerType);
        }

        learning.Approve(command.EpisodeKey, command.EmployerAccountId, command.FundingAccountId, command.LearnerKey, command.LearnerRef);

        await _learningDomainService.Update(learning);
    }
}
