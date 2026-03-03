using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsCommandHandler : ICommandHandler<UpdateEnglishAndMathsCommand>
{
    private readonly ILogger<UpdateEnglishAndMathsCommandHandler> _logger;
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateEnglishAndMathsCommandHandler(
        ILogger<UpdateEnglishAndMathsCommandHandler> logger,
        IApprenticeshipRepository apprenticeshipRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _apprenticeshipRepository = apprenticeshipRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateEnglishAndMathsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateEnglishAndMathsCommand for learning {LearningKey}", command.ApprenticeshipKey);

        var englishAndMathsCourses = BuildEnglishAndMathsCoursesWithInstalments(command);

        var learningDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        if (learningDomainModel == null)
        {
            _logger.LogError("No learning found for {LearningKey}", command.ApprenticeshipKey);
            throw new Exception($"No learning found for {command.ApprenticeshipKey} when handling {nameof(UpdateEnglishAndMathsCommand)}");
        }

        learningDomainModel.UpdateEnglishAndMathsCourses(englishAndMathsCourses, _systemClock);

        await _apprenticeshipRepository.Update(learningDomainModel);

        _logger.LogInformation("Successfully handled UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
    }

    private List<EnglishAndMaths> BuildEnglishAndMathsCoursesWithInstalments(UpdateEnglishAndMathsCommand command)
    {
        _logger.LogInformation("Building English and Maths details to domain models for apprenticeship {LearningKey}", command.ApprenticeshipKey);
        
        var courses = new List<EnglishAndMaths>();
        foreach (var detail in command.EnglishAndMathsDetails)
        {
            var course = new EnglishAndMaths(detail.StartDate, detail.EndDate, detail.Course, detail.LearnAimRef, detail.Amount, detail.WithdrawalDate, detail.CompletionDate, detail.PauseDate, detail.PriorLearningAdjustmentPercentage, detail.PeriodsInLearning);
            courses.Add(course);
        }

        _logger.LogInformation("{CourseCount} English and Maths courses built for apprenticeship {LearningKey}", courses.Count, command.ApprenticeshipKey);
        return courses;
    }
}