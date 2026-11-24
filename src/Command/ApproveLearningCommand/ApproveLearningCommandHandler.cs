using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;

public class ApproveLearningCommandHandler : ICommandHandler<ApproveLearningCommand>
{
    private readonly ILearningDomainService _learningDomainService;
    private readonly ILogger<ApproveLearningCommandHandler> _logger;
    private readonly ISystemClockService _systemClock;
    private readonly EmployerHiringGrantConfiguration _hiringGrantConfiguration;

    public ApproveLearningCommandHandler(ILearningDomainService learningDomainService, ILogger<ApproveLearningCommandHandler> logger, ISystemClockService systemClock, EmployerHiringGrantConfiguration hiringGrantConfiguration)
    {
        _learningDomainService = learningDomainService;
        _logger = logger;
        _systemClock = systemClock;
        _hiringGrantConfiguration = hiringGrantConfiguration;
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

            // employer type is only known at approval, so the hiring grant is generated here for every approved learner, opted in or not
            apprenticeshipLearning.GenerateHiringGrant(command.EpisodeKey, _hiringGrantConfiguration.StartDate, _systemClock);
        }

        learning.Approve(command.EpisodeKey, command.EmployerAccountId, command.FundingAccountId, command.LearnerKey, command.LearnerRef);

        await _learningDomainService.Update(learning);
    }
}
