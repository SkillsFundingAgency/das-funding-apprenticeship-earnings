namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

public class UpdateShortCourseOnProgrammeCommand : ICommand
{
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public UpdateShortCourseOnProgrammeRequest Request { get; set; }
    public UpdateShortCourseOnProgrammeCommand(Guid learningKey, Guid episodeKey, UpdateShortCourseOnProgrammeRequest request)
    {
        LearningKey = learningKey;
        EpisodeKey = episodeKey;
        Request = request;
    }
}
