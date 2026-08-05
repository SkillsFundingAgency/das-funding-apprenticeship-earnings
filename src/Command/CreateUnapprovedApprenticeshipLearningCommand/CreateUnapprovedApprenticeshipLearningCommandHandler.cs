using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
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
    private readonly IFundingBandMaximumService _fundingBandMaximumService;

    public CreateUnapprovedApprenticeshipLearningCommandHandler(
        ILogger<CreateUnapprovedApprenticeshipLearningCommandHandler> logger,
        ILearningFactory learningFactory,
        ILearningRepository learningRepository,
        ISystemClockService systemClock,
        IFundingBandMaximumService fundingBandMaximumService)
    {
        _logger = logger;
        _learningFactory = learningFactory;
        _learningRepository = learningRepository;
        _systemClock = systemClock;
        _fundingBandMaximumService = fundingBandMaximumService;
    }

    public async Task Handle(
        CreateUnapprovedApprenticeshipLearningCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        _logger.LogInformation("Handling CreateUnapprovedApprenticeshipLearningCommand for learning {LearningKey}", request.LearningKey);

        var fundingBandMaximum = await GetFundingBandMaximum(request);
        var learning = await _learningRepository.GetApprenticeshipLearning(request.LearningKey);

        if (learning != null && learning.HasEpisode(request.EpisodeKey))
        {
            UpdateAndCalculate(learning, request, fundingBandMaximum);
            await _learningRepository.Update(learning);
        }
        else if (learning != null)
        {
            learning.AddUnapprovedEpisode(
                request,
                fundingBandMaximum,
                request.ToEpisodePeriodsInLearning());

            UpdateAndCalculate(learning, request, fundingBandMaximum);
            await _learningRepository.Update(learning);
        }
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

    private async Task<int> GetFundingBandMaximum(CreateUnapprovedApprenticeshipLearningRequest request)
    {
        if (request.OnProgramme.FundingBandMaximum.HasValue)
        {
            return request.OnProgramme.FundingBandMaximum.Value;
        }

        var startDate = request.Prices.Min(x => x.StartDate);
        var courseCode = request.OnProgramme.TrainingCode;

        var fundingBandMaximum = await _fundingBandMaximumService.GetFundingBandMaximum(courseCode, startDate);

        if (!fundingBandMaximum.HasValue)
        {
            throw new Exception(
                $"No funding band maximum found for course {courseCode} for given StartDate {startDate}. LearningKey: {request.LearningKey}");
        }

        return fundingBandMaximum.Value;
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
                detail.PeriodsInLearning.Select(x => new DraftPeriodInLearning(x.StartDate, x.EndDate, x.OriginalExpectedEndDate))));
        }

        return courses;
    }
}

internal class DraftPeriodInLearning : IPeriodInLearning
{
    public DraftPeriodInLearning(DateTime startDate, DateTime? endDate, DateTime originalExpectedEndDate)
    {
        StartDate = startDate;
        EndDate = endDate;
        OriginalExpectedEndDate = originalExpectedEndDate;
    }

    public DateTime StartDate { get; }
    public DateTime? EndDate { get; }
    public DateTime OriginalExpectedEndDate { get; }
    public DateTime EffectiveEndDate => EndDate ?? OriginalExpectedEndDate;
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
