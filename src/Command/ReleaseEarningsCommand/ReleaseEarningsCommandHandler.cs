using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;

public class ReleaseEarningsCommandHandler : ICommandHandler<ReleaseEarningsCommand>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder _eventBuilder;
    private readonly IMessageSession _messageSession;
    private readonly PaymentsConfiguration _paymentsConfiguration;
    private readonly ILogger<ReleaseEarningsCommandHandler> _logger;

    public ReleaseEarningsCommandHandler(
        ILearningRepository learningRepository,
        IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder eventBuilder,
        IMessageSession messageSession,
        PaymentsConfiguration paymentsConfiguration,
        ILogger<ReleaseEarningsCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _eventBuilder = eventBuilder;
        _messageSession = messageSession;
        _paymentsConfiguration = paymentsConfiguration;
        _logger = logger;
    }

    public async Task Handle(ReleaseEarningsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{Handler} - Started for LearningKey: {LearningKey}", nameof(ReleaseEarningsCommandHandler), command.LearningKey);

        var learnerKey = command.Request.LearnerKey;
        var learnerRef = command.Request.LearnerRef;

        if (string.IsNullOrWhiteSpace(learnerRef))
        {
            _logger.LogInformation("{Handler} - Skipped for LearningKey: {LearningKey} as LearnerRef is not set", nameof(ReleaseEarningsCommandHandler), command.LearningKey);
            return;
        }

        var learning = await _learningRepository.GetApprenticeshipLearning(command.LearningKey);
        if (learning is null)
        {
            throw new InvalidOperationException($"Apprenticeship learning not found for key: {command.LearningKey}");
        }

        var options = new SendOptions();
        options.DoNotEnforceBestPractices();
        options.SetDestination(_paymentsConfiguration.PaymentsEndpoint);

        foreach (var episode in learning.Episodes.Where(e => !e.IsRemoved && e.IsApproved && e.EarningsProfile != null))
        {
            if (episode.FundingPlatform is not FundingPlatform.DAS)
            {
                _logger.LogInformation("{Handler} - Skipped EpisodeKey: {EpisodeKey} on LearningKey: {LearningKey} as FundingPlatform is not DAS", nameof(ReleaseEarningsCommandHandler), episode.EpisodeKey, command.LearningKey);
                continue;
            }

            await SendOnProgramme(episode, learning, learnerKey, learnerRef, options, cancellationToken);
            await SendEnglishAndMaths(episode, learning, learnerKey, learnerRef, options, cancellationToken);
        }

        _logger.LogInformation("{Handler} - Successfully processed and published events for LearningKey: {LearningKey}", nameof(ReleaseEarningsCommandHandler), command.LearningKey);
    }

    private async Task SendOnProgramme(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, Guid learnerKey, string learnerRef, SendOptions options, CancellationToken cancellationToken)
    {
        var paymentEvent = _eventBuilder.Build(episode, learning, episode.EmployerAccountId,
            episode.FundingEmployerAccountId ?? episode.EmployerAccountId, learnerKey, learnerRef);

        await _messageSession.Send(paymentEvent, options, cancellationToken);
        await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = paymentEvent }, cancellationToken: cancellationToken);
    }

    private async Task SendEnglishAndMaths(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, Guid learnerKey, string learnerRef, SendOptions options, CancellationToken cancellationToken)
    {
        foreach (var course in episode.EarningsProfile!.MathsAndEnglishCourses)
        {
            var englishAndMathsEvent = _eventBuilder.BuildForEnglishAndMaths(episode, learning, course, episode.EmployerAccountId,
                episode.FundingEmployerAccountId ?? episode.EmployerAccountId, learnerKey, learnerRef);

            await _messageSession.Send(englishAndMathsEvent, options, cancellationToken);
            await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = englishAndMathsEvent }, cancellationToken: cancellationToken);
        }
    }
}
