using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsCommandHandler : ICommandHandler<UpdateEnglishAndMathsCommand>
{
    private readonly ILogger<UpdateEnglishAndMathsCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateEnglishAndMathsCommandHandler(
        ILogger<UpdateEnglishAndMathsCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateEnglishAndMathsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);

        var mathsAndEnglishCourses = command.EnglishAndMathsDetails.Select(x =>
            MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(x.ToGenerateMathsAndEnglishPaymentsCommand())).ToList();

        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateMathsAndEnglishCourses(mathsAndEnglishCourses, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Successfully handled UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
    }
}