using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand;

public class SendApprenticeshipPayableEarningsToPaymentsCommandHandler : ICommandHandler<SendApprenticeshipPayableEarningsToPaymentsCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder _eventBuilder;
    private readonly IMessageSession _messageSession;
    private readonly PaymentsConfiguration _paymentsConfiguration;
    private readonly ILogger<SendApprenticeshipPayableEarningsToPaymentsCommandHandler> _logger;

    public SendApprenticeshipPayableEarningsToPaymentsCommandHandler(
        ILearningRepository learningRepository,
        IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder eventBuilder,
        IMessageSession messageSession,
        PaymentsConfiguration paymentsConfiguration,
        ILogger<SendApprenticeshipPayableEarningsToPaymentsCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _eventBuilder = eventBuilder;
        _messageSession = messageSession;
        _paymentsConfiguration = paymentsConfiguration;
        _logger = logger;
    }

    public async Task Handle(SendApprenticeshipPayableEarningsToPaymentsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{Handler} - Started for LearningKey: {LearningKey}", nameof(SendApprenticeshipPayableEarningsToPaymentsCommandHandler), command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey);

        var learning = await _learningRepository.GetApprenticeshipLearning(command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey);
        if (learning == null)
        {
            throw new InvalidOperationException($"Apprenticeship learning not found for key: {command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey}");
        }

        var episode = learning.Episodes.SingleOrDefault(x => x.EpisodeKey == command.ApprenticeshipPayableEarningsUpdatedEvent.EpisodeKey);
        if (episode == null)
        {
            throw new InvalidOperationException($"Apprenticeship episode not found for EpisodeKey: {command.ApprenticeshipPayableEarningsUpdatedEvent.EpisodeKey} on LearningKey: {command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey}");
        }

        var employerAccountId = command.ApprenticeshipPayableEarningsUpdatedEvent.EmployerAccountId;
        var fundingAccountId = command.ApprenticeshipPayableEarningsUpdatedEvent.FundingAccountId;
        var learnerKey = command.ApprenticeshipPayableEarningsUpdatedEvent.LearnerKey;
        var learnerRef = command.ApprenticeshipPayableEarningsUpdatedEvent.LearnerRef;

        var paymentEvent = _eventBuilder.Build(
            episode,
            learning,
            employerAccountId,
            fundingAccountId,
            learnerKey,
            learnerRef);

        var options = new SendOptions();
        options.DoNotEnforceBestPractices();
        options.SetDestination(_paymentsConfiguration.PaymentsEndpoint);
        await _messageSession.Send(paymentEvent, options, cancellationToken);

        await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = paymentEvent }, cancellationToken: cancellationToken);

        if (episode.EarningsProfile != null)
        {
            foreach (var course in episode.EarningsProfile.MathsAndEnglishCourses.Where(c => c.Instalments.Any()))
            {
                var englishAndMathsPaymentEvent = _eventBuilder.BuildForEnglishAndMaths(episode, learning, course, employerAccountId, fundingAccountId, learnerKey, learnerRef);
                await _messageSession.Send(englishAndMathsPaymentEvent, options, cancellationToken);
                await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = englishAndMathsPaymentEvent }, cancellationToken: cancellationToken);
            }
        }

        _logger.LogInformation("{Handler} - Successfully processed and published event for LearningKey: {LearningKey}", nameof(SendApprenticeshipPayableEarningsToPaymentsCommandHandler), command.ApprenticeshipPayableEarningsUpdatedEvent.LearningKey);
    }
}
