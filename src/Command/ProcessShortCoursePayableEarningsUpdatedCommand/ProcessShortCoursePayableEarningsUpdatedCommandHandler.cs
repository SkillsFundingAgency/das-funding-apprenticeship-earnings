using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand;

public class ProcessShortCoursePayableEarningsUpdatedCommandHandler : ICommandHandler<ProcessShortCoursePayableEarningsUpdatedCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder _eventBuilder;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<ProcessShortCoursePayableEarningsUpdatedCommandHandler> _logger;

    public ProcessShortCoursePayableEarningsUpdatedCommandHandler(
        ILearningRepository learningRepository,
        IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder eventBuilder,
        IMessageSession messageSession,
        ILogger<ProcessShortCoursePayableEarningsUpdatedCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _eventBuilder = eventBuilder;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(ProcessShortCoursePayableEarningsUpdatedCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{handler} - Started for LearningKey: {LearningKey}", nameof(ProcessShortCoursePayableEarningsUpdatedCommandHandler), command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);

        var learning = await _learningRepository.GetShortCourseLearning(command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);
        if (learning == null)
        {
            throw new InvalidOperationException($"Short course learning not found for key: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
        }

        var episode = learning.GetEpisode();
        if (episode == null)
        {
            throw new InvalidOperationException($"Short course episode not found for LearningKey: {command.ShortCoursePayableEarningsUpdatedEvent.LearningKey}");
        }

        if (episode.EpisodeKey != command.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey)
        {
            throw new InvalidOperationException($"Short course episode key mismatch. Expected: {command.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey}, Found: {episode.EpisodeKey}");
        }

        var paymentEvent = _eventBuilder.Build(episode, learning);

        await _messageSession.Publish(paymentEvent);

        _logger.LogInformation("{handler} - Successfully processed and published event for LearningKey: {LearningKey}", nameof(ProcessShortCoursePayableEarningsUpdatedCommandHandler), command.ShortCoursePayableEarningsUpdatedEvent.LearningKey);
    }
}
