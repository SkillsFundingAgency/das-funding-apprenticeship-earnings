using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsCommandHandler : ICommandHandler<UpdateEnglishAndMathsCommand>
{
    private readonly ILogger<UpdateEnglishAndMathsCommandHandler> _logger;
    private readonly ILearningRepository _learningRepository;
    private readonly ISystemClockService _systemClock;

    public UpdateEnglishAndMathsCommandHandler(
        ILogger<UpdateEnglishAndMathsCommandHandler> logger,
        ILearningRepository learningRepository,
        ISystemClockService systemClock)
    {
        _logger = logger;
        _learningRepository = learningRepository;
        _systemClock = systemClock;
    }

    public async Task Handle(UpdateEnglishAndMathsCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateEnglishAndMathsCommand for learning {LearningKey}", command.LearningKey);

        var englishAndMathsCourses = BuildEnglishAndMathsCoursesWithInstalments(command);

        var learningDomainModel = await _learningRepository.GetApprenticeshipLearning(command.LearningKey);

        if (learningDomainModel == null)
        {
            _logger.LogError("No learning found for {LearningKey}", command.LearningKey);
            throw new Exception($"No learning found for {command.LearningKey} when handling {nameof(UpdateEnglishAndMathsCommand)}");
        }

        learningDomainModel.UpdateEnglishAndMathsCourses(englishAndMathsCourses, _systemClock);

        await _learningRepository.Update(learningDomainModel);

        _logger.LogInformation("Successfully handled UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.LearningKey);
    }

    private List<EnglishAndMaths> BuildEnglishAndMathsCoursesWithInstalments(UpdateEnglishAndMathsCommand command)
    {
        _logger.LogInformation("Building English and Maths details to domain models for apprenticeship {LearningKey}", command.LearningKey);
        
        var courses = new List<EnglishAndMaths>();
        foreach (var detail in command.EnglishAndMathsDetails)
        {
            var course = new EnglishAndMaths(detail.StartDate, detail.EndDate, detail.Course, detail.LearnAimRef, detail.Amount, detail.WithdrawalDate, detail.CompletionDate, detail.PauseDate, detail.PriorLearningAdjustmentPercentage, detail.PeriodsInLearning);
            courses.Add(course);
        }

        _logger.LogInformation("{CourseCount} English and Maths courses built for apprenticeship {LearningKey}", courses.Count, command.LearningKey);
        return courses;
    }
}