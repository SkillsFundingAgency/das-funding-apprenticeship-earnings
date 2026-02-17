using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;

public class CreateUnapprovedShortCourseLearningCommand : ICommand
{
    public CreateUnapprovedShortCourseLearningRequest Request { get; set; }
    public CreateUnapprovedShortCourseLearningCommand(CreateUnapprovedShortCourseLearningRequest request)
    {
        Request = request;
    }
}

public class CreateUnapprovedShortCourseLearningCommandHandler
    : ICommandHandler<CreateUnapprovedShortCourseLearningCommand>
{
    private readonly ILogger<CreateUnapprovedShortCourseLearningCommandHandler> _logger;
    private readonly ISystemClockService _systemClockService;
    private IApprenticeshipFactory _apprenticeshipFactory;
    private IApprenticeshipRepository _apprenticeshipRepository;

    public CreateUnapprovedShortCourseLearningCommandHandler(
        ILogger<CreateUnapprovedShortCourseLearningCommandHandler> logger,
        ISystemClockService systemClockService, IApprenticeshipFactory apprenticeshipFactory, IApprenticeshipRepository apprenticeshipRepository)
    {
        _logger = logger;
        _systemClockService = systemClockService;
        _apprenticeshipFactory = apprenticeshipFactory;
        _apprenticeshipRepository = apprenticeshipRepository;
    }

    public async Task Handle(
        CreateUnapprovedShortCourseLearningCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling CreateUnapprovedShortCourseLearningCommand for learning {LearningKey}",
            command.Request.LearningKey);

        var shortCourse = _apprenticeshipFactory.CreateNewShortCourse(command.Request);

        shortCourse.ApprenticeshipEpisodes.Single().CalculateShortCourseOnProgram(shortCourse, _systemClockService, false, JsonSerializer.Serialize(command.Request));

        await _apprenticeshipRepository.Add(shortCourse);

        _logger.LogInformation(
            "Successfully handled CreateUnapprovedShortCourseLearningCommand for learning {LearningKey}",
            command.Request.LearningKey);
    }
}
