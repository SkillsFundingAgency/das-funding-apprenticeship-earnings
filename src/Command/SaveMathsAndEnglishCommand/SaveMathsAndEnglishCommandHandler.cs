using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public class SaveMathsAndEnglishCommandHandler : ICommandHandler<SaveMathsAndEnglishCommand>
{
    private readonly ILogger<SaveMathsAndEnglishCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public SaveMathsAndEnglishCommandHandler(
        ILogger<SaveMathsAndEnglishCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(SaveMathsAndEnglishCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveMathsAndEnglishCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);

        var mathsAndEnglishCourses = command.MathsAndEnglishDetails.Select(x =>
            MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(x.ToGenerateMathsAndEnglishPaymentsCommand())).ToList();

        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateMathsAndEnglishCourses(mathsAndEnglishCourses, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Successfully handled SaveLearningSupportCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
    }
}