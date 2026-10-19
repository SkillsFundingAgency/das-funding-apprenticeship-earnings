using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;
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
        _logger.LogInformation("{Handler} - Started for LearningKey: {LearningKey} with ReleaseType: {ReleaseType} and {CourseKeyCount} English and Maths course keys",
            nameof(ReleaseEarningsCommandHandler), command.LearningKey, command.Request.ReleaseType, command.Request.EnglishAndMathsCourseKeys.Count);

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

        foreach (var episode in GetEpisodesToRelease(learning))
        {
            if (episode.FundingPlatform is not FundingPlatform.DAS)
            {
                _logger.LogInformation("{Handler} - Skipped EpisodeKey: {EpisodeKey} on LearningKey: {LearningKey} as FundingPlatform is not DAS", nameof(ReleaseEarningsCommandHandler), episode.EpisodeKey, command.LearningKey);
                continue;
            }

            if (command.Request.ReleaseType is ReleaseType.All or ReleaseType.OnProgramme)
            {
                await SendOnProgramme(episode, learning, learnerKey, learnerRef, options, cancellationToken);
            }

            if (command.Request.ReleaseType is ReleaseType.All or ReleaseType.FunctionalSkill)
            {
                await SendEnglishAndMaths(episode, learning, learnerKey, learnerRef, command.Request, options, cancellationToken);
            }
        }

        _logger.LogInformation("{Handler} - Successfully processed and published events for LearningKey: {LearningKey}", nameof(ReleaseEarningsCommandHandler), command.LearningKey);
    }

    /// <summary>
    /// Removed episodes are only released when the learning has no approved episode still active, so payments are told
    /// that all earnings have been removed without overriding the earnings of an episode still active on the same learning.
    /// </summary>
    private static List<ApprenticeshipEpisode> GetEpisodesToRelease(ApprenticeshipLearning learning)
    {
        var approvedEpisodes = learning.Episodes.Where(e => e.IsApproved && e.EarningsProfile != null).ToList();
        var activeEpisodes = approvedEpisodes.Where(e => !e.IsRemoved).ToList();

        return activeEpisodes.Count > 0 ? activeEpisodes : approvedEpisodes;
    }

    private async Task SendOnProgramme(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, Guid learnerKey, string learnerRef, SendOptions options, CancellationToken cancellationToken)
    {
        var paymentEvent = _eventBuilder.Build(episode, learning, episode.EmployerAccountId,
            episode.FundingEmployerAccountId ?? episode.EmployerAccountId, learnerKey, learnerRef);

        await _messageSession.Send(paymentEvent, options, cancellationToken);
        await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = paymentEvent }, cancellationToken: cancellationToken);
    }

    private async Task SendEnglishAndMaths(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, Guid learnerKey, string learnerRef, ReleaseEarningsRequest request, SendOptions options, CancellationToken cancellationToken)
    {
        foreach (var course in GetEnglishAndMathsCoursesToRelease(episode, request))
        {
            var englishAndMathsEvent = _eventBuilder.BuildForEnglishAndMaths(episode, learning, course, episode.EmployerAccountId,
                episode.FundingEmployerAccountId ?? episode.EmployerAccountId, learnerKey, learnerRef);

            await _messageSession.Send(englishAndMathsEvent, options, cancellationToken);
            await _messageSession.Publish(new GrowthAndSkillsPaymentsRecalculatedEvent { Command = englishAndMathsEvent }, cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// When course keys are supplied only those courses are released, including any that have been removed
    /// (e.g. omitted from the latest submission) so that payments can clear the earnings previously sent for them.
    /// When no course keys are supplied (e.g. on approval), all active courses are released for a ReleaseType of All
    /// and none for FunctionalSkill. Removed courses are not released in this case as payments has never received them.
    /// </summary>
    private static IEnumerable<EnglishAndMathsDomainModel> GetEnglishAndMathsCoursesToRelease(ApprenticeshipEpisode episode, ReleaseEarningsRequest request)
    {
        var courses = episode.EarningsProfile!.MathsAndEnglishCourses;

        if (request.EnglishAndMathsCourseKeys.Count == 0)
        {
            return request.ReleaseType == ReleaseType.All ? courses.Where(c => !c.IsRemoved) : [];
        }

        return courses.Where(c => request.EnglishAndMathsCourseKeys.Contains(c.Key));
    }
}
