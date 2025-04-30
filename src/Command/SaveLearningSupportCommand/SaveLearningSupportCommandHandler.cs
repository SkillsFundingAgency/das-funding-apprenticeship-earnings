using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;

public class SaveLearningSupportCommandHandler : ICommandHandler<SaveLearningSupportCommand>
{
    private readonly ILogger<SaveCareDetailsCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClockService;
    private readonly IMessageSession _messageSession;
    private readonly IApprenticeshipEarningsRecalculatedEventBuilder _earningsRecalculatedEventBuilder;

    public SaveLearningSupportCommandHandler(
        ILogger<SaveCareDetailsCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock,
        IMessageSession messageSession,
        IApprenticeshipEarningsRecalculatedEventBuilder earningsRecalculatedEventBuilder)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClockService = systemClock;
        _messageSession = messageSession;
        _earningsRecalculatedEventBuilder = earningsRecalculatedEventBuilder;
    }

    public async Task Handle(SaveLearningSupportCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling SaveLearningSupportCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);

        var apprenticeshipDomainModel = await GetDomainApprenticeship(command.ApprenticeshipKey);

        apprenticeshipDomainModel.AddAdditionalEarnings(
            command.LearningSupportPayments.Select(x=> new AdditionalPayment(
                x.AcademicYear, 
                x.DeliveryPeriod,
                AdditionalPaymentAmounts.LearningSupport,
                x.AcademicYear.ToDateTime(x.DeliveryPeriod),
                InstalmentTypes.LearningSupport
            )).ToList()
            , _systemClockService);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Publishing EarningsRecalculatedEvent for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
        await _messageSession.Publish(_earningsRecalculatedEventBuilder.Build(apprenticeshipDomainModel));

        _logger.LogInformation("Successfully handled SaveLearningSupportCommand for apprenticeship {apprenticeshipKey}", command.ApprenticeshipKey);
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
