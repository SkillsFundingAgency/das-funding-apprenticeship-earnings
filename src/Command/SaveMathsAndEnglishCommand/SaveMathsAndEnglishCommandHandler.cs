using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public class SaveMathsAndEnglishCommandHandler : ICommandHandler<SaveMathsAndEnglishCommand>
{
    private readonly ILogger<SaveMathsAndEnglishCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _earningsRecalculatedEventBuilder;
    private readonly ISystemClockService _systemClock;

    public SaveMathsAndEnglishCommandHandler(
        ILogger<SaveMathsAndEnglishCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        IMessageSession messageSession,
        IApprenticeshipEarningsRecalculatedEventBuilder earningsRecalculatedEventBuilder,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _messageSession = messageSession;
        _earningsRecalculatedEventBuilder = earningsRecalculatedEventBuilder;
        _systemClock = systemClock;
    }

    public async Task Handle(SaveMathsAndEnglishCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveMathsAndEnglishCommand for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);

        var mathsAndEnglishCourses = command.MathsAndEnglishDetails.Select(x =>
            MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(new GenerateMathsAndEnglishPaymentsCommand(x.StartDate, x.EndDate, x.Course, x.Amount, x.ActualEndDate))).ToList();

        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateMathsAndEnglishCourses(mathsAndEnglishCourses, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
        await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel), cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully handled SaveLearningSupportCommand for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
    }
}