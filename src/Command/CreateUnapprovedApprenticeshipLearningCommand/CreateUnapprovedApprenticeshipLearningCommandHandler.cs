using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;
using EnglishAndMathsAdditionalPayment = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMathsAdditionalPayment;
using AdditionalPayment = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship.AdditionalPayment;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand;

public class CreateUnapprovedApprenticeshipLearningCommandHandler
    : ICommandHandler<CreateUnapprovedApprenticeshipLearningCommand>
{
    private readonly ILogger<CreateUnapprovedApprenticeshipLearningCommandHandler> _logger;
    private readonly ILearningFactory _learningFactory;
    private readonly ILearningRepository _learningRepository;
    private readonly ISystemClockService _systemClock;
    private readonly ApprenticeshipOptInConfiguration _apprenticeshipOptInConfiguration;

    public CreateUnapprovedApprenticeshipLearningCommandHandler(
        ILogger<CreateUnapprovedApprenticeshipLearningCommandHandler> logger,
        ILearningFactory learningFactory,
        ILearningRepository learningRepository,
        ISystemClockService systemClock,
        ApprenticeshipOptInConfiguration apprenticeshipOptInConfiguration)
    {
        _logger = logger;
        _learningFactory = learningFactory;
        _learningRepository = learningRepository;
        _systemClock = systemClock;
        _apprenticeshipOptInConfiguration = apprenticeshipOptInConfiguration;
    }

    public async Task Handle(
        CreateUnapprovedApprenticeshipLearningCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        _logger.LogInformation("Handling CreateUnapprovedApprenticeshipLearningCommand for learning {LearningKey}", request.LearningKey);

        var fundingBandMaximum = request.OnProgramme.FundingBandMaximum
                                 ?? throw new ArgumentException(
                                     $"FundingBandMaximum is required for draft apprenticeship learning. LearningKey: {request.LearningKey}");
        var learning = await _learningRepository.GetApprenticeshipLearning(request.LearningKey);

        if (learning != null && learning.HasEpisode(request.EpisodeKey))
        {
            var handled = await HandleExistingLearningExistingEpisode(learning, request, fundingBandMaximum);
            if (!handled) return;
        }
        else if (learning != null)
        {
            await HandleExistingLearningNewEpisode(learning, request, fundingBandMaximum);
        }
        else if (request.IsNewApprenticeshipLearner)
        {
            await HandleNewLearningAndEpisode(request, fundingBandMaximum);
        }

        _logger.LogInformation(
            "Successfully handled CreateUnapprovedApprenticeshipLearningCommand for learning {LearningKey}",
            request.LearningKey);
    }

    /// <summary>
    /// Returns false when the episode is approved and the learner is opted out, in which case nothing is changed.
    /// </summary>
    private async Task<bool> HandleExistingLearningExistingEpisode(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        if (!IsOptedIntoEarnings(request))
        {
            var episode = learning.GetEpisode(request.EpisodeKey);
            if (episode.IsApproved)
            {
                _logger.LogInformation("Apprenticeship did not meet opt-in criteria for learning {LearningKey}, episode is approved so it is left unchanged", request.LearningKey);
                return false;
            }

            _logger.LogInformation("Apprenticeship did not meet opt-in criteria for learning {LearningKey}, updating the episode without earnings", request.LearningKey);

            UpdateWithoutCalculate(learning, request, fundingBandMaximum);
        }
        else
        {
            UpdateAndCalculate(learning, request, fundingBandMaximum);
        }

        await _learningRepository.Update(learning);
        return true;
    }

    private async Task HandleExistingLearningNewEpisode(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        learning.AddUnapprovedEpisode(
            request,
            fundingBandMaximum,
            request.ToEpisodePeriodsInLearning());

        if (!IsOptedIntoEarnings(request))
        {
            _logger.LogInformation("Apprenticeship did not meet opt-in criteria for learning {LearningKey}, storing the new episode without earnings", request.LearningKey);

            // the new episode's FundingPlatform defaults to SLD
            UpdateWithoutCalculate(learning, request, fundingBandMaximum);
        }
        else
        {
            UpdateAndCalculate(learning, request, fundingBandMaximum);
        }

        await _learningRepository.Update(learning);
    }

    private async Task HandleNewLearningAndEpisode(
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        if (!IsOptedIntoEarnings(request))
        {
            _logger.LogInformation("Apprenticeship did not meet opt-in criteria for learning {LearningKey}, storing it without earnings", request.LearningKey);

            var optedOutLearning = _learningFactory.CreateNewUnapprovedApprenticeship(request, fundingBandMaximum, FundingPlatform.SLD);

            UpdateWithoutCalculate(optedOutLearning, request, fundingBandMaximum);
            await _learningRepository.Add(optedOutLearning);
            return;
        }

        var fundingPlatform = IsOptedIntoPayments(request) ? FundingPlatform.DAS : FundingPlatform.SLD;
        var newLearning = _learningFactory.CreateNewUnapprovedApprenticeship(request, fundingBandMaximum, fundingPlatform);

        UpdateAndCalculate(newLearning, request, fundingBandMaximum);
        await _learningRepository.Add(newLearning);
    }

    private bool IsOptedIntoEarnings(CreateUnapprovedApprenticeshipLearningRequest request)
    {
        return _apprenticeshipOptInConfiguration.StartDate <= request.PeriodsInLearning.Min(x => x.StartDate)
            && _apprenticeshipOptInConfiguration.EarningsOptedInProviders.Contains(request.OnProgramme.Ukprn);
    }

    private bool IsOptedIntoPayments(CreateUnapprovedApprenticeshipLearningRequest request)
    {
        return _apprenticeshipOptInConfiguration.PaymentsOptedInProviders.Contains(request.OnProgramme.Ukprn);
    }

    /// <summary>
    /// Opted in: updates the learning, calculates the on-programme earnings, then English and Maths and Learning Support with earnings.
    /// </summary>
    private void UpdateAndCalculate(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        UpdateLearningInformation(learning, request, fundingBandMaximum);

        learning.Calculate(
            _systemClock,
            JsonSerializer.Serialize(request),
            request.EpisodeKey,
            initialGenerationIsApproved: false);

        var episode = learning.GetEpisode(request.EpisodeKey);
        episode.UpdateEnglishAndMaths(BuildEnglishAndMathsCourses(request, calculate: true), _systemClock);
        episode.AddAdditionalEarnings(GenerateLearningSupport(request.LearningSupport, calculate: true), InstalmentTypes.LearningSupport, _systemClock);
    }

    /// <summary>
    /// Opted out: updates the learning and stores a blank earnings profile, with English and Maths and Learning Support
    /// stored but no earnings calculated.
    /// </summary>
    private void UpdateWithoutCalculate(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        UpdateLearningInformation(learning, request, fundingBandMaximum);

        var episode = learning.GetEpisode(request.EpisodeKey);
        episode.UpdateWithoutCalculating(learning, _systemClock);
        episode.UpdateEnglishAndMaths(BuildEnglishAndMathsCourses(request, calculate: false), _systemClock);
        episode.AddAdditionalEarnings(GenerateLearningSupport(request.LearningSupport, calculate: false), InstalmentTypes.LearningSupport, _systemClock);
    }

    private void UpdateLearningInformation(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        learning.UpdateUnapprovedApprenticeshipInformation(
            request,
            fundingBandMaximum,
            request.ToEpisodePeriodsInLearning(),
            _systemClock);
    }

    private static List<AdditionalPayment> GenerateLearningSupport(IEnumerable<LearningSupportItem> learningSupport, bool calculate)
    {
        return learningSupport
            .SelectMany(x => calculate
                ? LearningSupportPayments.GenerateLearningSupportPayments(x.StartDate, x.EndDate)
                : LearningSupportPayments.GenerateLearningSupportPaymentsWithoutEarnings(x.StartDate, x.EndDate))
            .DistinctBy(x => new { x.AcademicYear, x.DeliveryPeriod, x.DueDate })
            .ToList();
    }

    private static List<EnglishAndMathsDomainModel> BuildEnglishAndMathsCourses(CreateUnapprovedApprenticeshipLearningRequest request, bool calculate)
    {
        var courses = new List<EnglishAndMathsDomainModel>();

        foreach (var detail in request.EnglishAndMaths)
        {
            var learningSupportPayments = GenerateLearningSupport(detail.LearningSupport, calculate)
                .Select(p => new EnglishAndMathsAdditionalPayment(p.AcademicYear, p.DeliveryPeriod, p.Amount, p.DueDate, p.AdditionalPaymentType))
                .ToList();

            var periodsInLearning = detail.PeriodsInLearning.Select(x => new PeriodInLearningItem
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                OriginalExpectedEndDate = x.OriginalExpectedEndDate
            }).ToList();

            courses.Add(calculate
                ? new EnglishAndMathsDomainModel(
                    detail.StartDate,
                    detail.EndDate,
                    detail.Course,
                    detail.LearnAimRef,
                    detail.Amount,
                    detail.WithdrawalDate,
                    detail.CompletionDate,
                    detail.PauseDate,
                    detail.CombinedFundingAdjustmentPercentage,
                    periodsInLearning,
                    learningSupportPayments)
                : EnglishAndMathsDomainModel.CreateWithoutEarnings(
                    detail.StartDate,
                    detail.EndDate,
                    detail.Course,
                    detail.LearnAimRef,
                    detail.Amount,
                    detail.WithdrawalDate,
                    detail.CompletionDate,
                    detail.PauseDate,
                    detail.CombinedFundingAdjustmentPercentage,
                    periodsInLearning,
                    learningSupportPayments));
        }

        return courses;
    }
}

internal static class CreateUnapprovedApprenticeshipLearningRequestExtensions
{
    public static List<ApprenticeshipPeriodInLearning> ToEpisodePeriodsInLearning(
        this CreateUnapprovedApprenticeshipLearningRequest request)
    {
        return request.PeriodsInLearning
            .Select(x => new ApprenticeshipPeriodInLearning(
                request.EpisodeKey,
                x.StartDate,
                x.EndDate,
                x.OriginalExpectedEndDate))
            .ToList();
    }
}
