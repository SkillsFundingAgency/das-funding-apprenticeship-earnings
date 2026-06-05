using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand;

public class SendShortCoursePayableEarningsToPaymentsCommandHandler : ICommandHandler<SendShortCoursePayableEarningsToPaymentsCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder _eventBuilder;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<SendShortCoursePayableEarningsToPaymentsCommandHandler> _logger;

    public SendShortCoursePayableEarningsToPaymentsCommandHandler(
        ILearningRepository learningRepository,
        IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder eventBuilder,
        IMessageSession messageSession,
        ILogger<SendShortCoursePayableEarningsToPaymentsCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _eventBuilder = eventBuilder;
        _messageSession = messageSession;
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

        var paymentEvent = _eventBuilder.Build(episode, learning);

        await _messageSession.Publish(paymentEvent, cancellationToken: cancellationToken);

        _logger.LogInformation("{handler} - Successfully processed and published event for LearningKey: {LearningKey}", nameof(SendShortCoursePayableEarningsToPaymentsCommandHandler), command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);
    }
}
