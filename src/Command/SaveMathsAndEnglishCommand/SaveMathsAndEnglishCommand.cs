using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public class SaveMathsAndEnglishCommand : ICommand
{
    public SaveMathsAndEnglishCommand(Guid apprenticeshipKey, SaveMathsAndEnglishRequest saveMathsAndEnglishRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        MathsAndEnglishDetails = saveMathsAndEnglishRequest;
    }

    public Guid ApprenticeshipKey { get; }

    public List<MathsAndEnglishDetail> MathsAndEnglishDetails { get; set; }
}

public class MathsAndEnglishDetail
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;

    public decimal Amount { get; set; }
}

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
            MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(x.StartDate, x.EndDate, x.Course, x.Amount)).ToList();

        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateMathsAndEnglishCourses(mathsAndEnglishCourses, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
        await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel), cancellationToken: cancellationToken);

        _logger.LogInformation("Successfully handled SaveLearningSupportCommand for apprenticeship {ApprenticeshipKey}", command.ApprenticeshipKey);
    }
}