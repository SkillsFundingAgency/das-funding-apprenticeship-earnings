using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
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

    public CreateUnapprovedApprenticeshipLearningCommandHandler(
        ILogger<CreateUnapprovedApprenticeshipLearningCommandHandler> logger,
        ILearningFactory learningFactory,
        ILearningRepository learningRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _learningFactory = learningFactory;
        _learningRepository = learningRepository;
        _systemClock = systemClock;
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

        //existing learning, existing episode
        if (learning != null && learning.HasEpisode(request.EpisodeKey))
        {
            UpdateAndCalculate(learning, request, fundingBandMaximum);
            await _learningRepository.Update(learning);
        }
        //existing learning, new episode
        else if (learning != null)
        {
            learning.AddUnapprovedEpisode(
                request,
                fundingBandMaximum,
                request.ToEpisodePeriodsInLearning());

            UpdateAndCalculate(learning, request, fundingBandMaximum);
            await _learningRepository.Update(learning);
        }
        //new learning & episode
        else
        {
            var newLearning = _learningFactory.CreateNewUnapprovedApprenticeship(request, fundingBandMaximum);

            UpdateAndCalculate(newLearning, request, fundingBandMaximum);
            await _learningRepository.Add(newLearning);
        }

        _logger.LogInformation(
            "Successfully handled CreateUnapprovedApprenticeshipLearningCommand for learning {LearningKey}",
            request.LearningKey);
    }

    private void UpdateAndCalculate(
        ApprenticeshipLearning learning,
        CreateUnapprovedApprenticeshipLearningRequest request,
        int fundingBandMaximum)
    {
        learning.UpdateUnapprovedApprenticeshipInformation(
            request,
            fundingBandMaximum,
            request.ToEpisodePeriodsInLearning(),
            _systemClock);

        learning.Calculate(
            _systemClock,
            JsonSerializer.Serialize(request),
            request.EpisodeKey,
            initialGenerationIsApproved: false);

        var episode = learning.GetEpisode(request.EpisodeKey);
        episode.UpdateEnglishAndMaths(BuildEnglishAndMathsCourses(request), _systemClock);

        var learningSupportPayments = request.LearningSupport
            .SelectMany(x => LearningSupportPayments.GenerateLearningSupportPayments(x.StartDate, x.EndDate))
            .DistinctBy(x => new { x.AcademicYear, x.DeliveryPeriod, x.DueDate })
            .ToList();

        episode.AddAdditionalEarnings(learningSupportPayments, InstalmentTypes.LearningSupport, _systemClock);
    }

    private static List<EnglishAndMathsDomainModel> BuildEnglishAndMathsCourses(CreateUnapprovedApprenticeshipLearningRequest request)
    {
        var courses = new List<EnglishAndMathsDomainModel>();

        foreach (var detail in request.EnglishAndMaths)
        {
            courses.Add(new EnglishAndMathsDomainModel(
                detail.StartDate,
                detail.EndDate,
                detail.Course,
                detail.LearnAimRef,
                detail.Amount,
                detail.WithdrawalDate,
                detail.CompletionDate,
                detail.PauseDate,
                detail.CombinedFundingAdjustmentPercentage,
                detail.PeriodsInLearning.Select(x => new PeriodInLearningItem
                {
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    OriginalExpectedEndDate = x.OriginalExpectedEndDate
                })));
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
