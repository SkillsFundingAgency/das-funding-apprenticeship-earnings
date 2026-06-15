using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand;

public class SendShortCoursePayableEarningsToPaymentsCommandHandler : ICommandHandler<SendShortCoursePayableEarningsToPaymentsCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder _eventBuilder;
    private readonly IMessageSession _messageSession;
    private readonly PaymentsConfiguration _paymentsConfiguration;
    private readonly ILogger<SendShortCoursePayableEarningsToPaymentsCommandHandler> _logger;

    public SendShortCoursePayableEarningsToPaymentsCommandHandler(
        ILearningRepository learningRepository,
        IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder eventBuilder,
        IMessageSession messageSession,
        PaymentsConfiguration paymentsConfiguration,
        ILogger<SendShortCoursePayableEarningsToPaymentsCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _eventBuilder = eventBuilder;
        _messageSession = messageSession;
        _paymentsConfiguration = paymentsConfiguration;
        _logger = logger;
    }

    public async Task Handle(SendShortCoursePayableEarningsToPaymentsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{handler} - Started for LearningKey: {LearningKey}", nameof(SendShortCoursePayableEarningsToPaymentsCommandHandler), command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);

        var learning = await _learningRepository.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);
        if (learning == null)
        {
            throw new InvalidOperationException($"Short course learning not found for key: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
        }

        var episode = learning.Episodes.SingleOrDefault(x => x.EpisodeKey == command.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey);
        if (episode == null)
        {
            throw new InvalidOperationException($"Short course episode not found for EpisodeKey: {command.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey} on LearningKey: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
        }

        var paymentEvent = _eventBuilder.Build(episode, learning, command.ShortCoursePayableEarningsUpdatedEvent.EmployerAccountId, command.ShortCoursePayableEarningsUpdatedEvent.FundingAccountId);

        var options = new SendOptions();
        options.DoNotEnforceBestPractices();
        options.SetDestination(_paymentsConfiguration.PaymentsEndpoint);
        await _messageSession.Send(paymentEvent, options);

        await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = paymentEvent }, cancellationToken: cancellationToken);

        _logger.LogInformation("{handler} - Successfully processed and published event for LearningKey: {LearningKey}", nameof(SendShortCoursePayableEarningsToPaymentsCommandHandler), command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);
    }
}
