using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
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
        _logger.LogInformation("Handling UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);

        var englishAndMathsCourses = BuildMathsAndEnglishCoursesWithInstalments(command);

        var apprenticeshipDomainModel = await _apprenticeshipRepository.Get(command.ApprenticeshipKey);

        apprenticeshipDomainModel.UpdateMathsAndEnglishCourses(englishAndMathsCourses, _systemClock);

        await _apprenticeshipRepository.Update(apprenticeshipDomainModel);

        _logger.LogInformation("Successfully handled UpdateEnglishAndMathsCommand for apprenticeship {LearningKey}", command.ApprenticeshipKey);
    }

    private List<MathsAndEnglish> BuildMathsAndEnglishCoursesWithInstalments(UpdateEnglishAndMathsCommand command)
    {
        _logger.LogInformation("Mapping English and Maths details to domain models for apprenticeship {LearningKey}", command.ApprenticeshipKey);
        
        var courses = new List<MathsAndEnglish>();
        foreach (var detail in command.EnglishAndMathsDetails)
        {
            var course = new MathsAndEnglish(detail.StartDate, detail.EndDate, detail.Course, detail.Amount, detail.WithdrawalDate, detail.ActualEndDate, detail.PauseDate, detail.PriorLearningAdjustmentPercentage);
            courses.Add(course);
        }

        _logger.LogInformation("Mapped {CourseCount} English and Maths courses for apprenticeship {LearningKey}", courses.Count, command.ApprenticeshipKey);
        return courses;
    }
}