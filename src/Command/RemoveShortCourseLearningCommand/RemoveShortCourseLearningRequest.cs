namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;

public class RemoveShortCourseLearningRequest
{
    public Guid LearnerKey { get; set; }
    public string LearnerRef { get; set; } = string.Empty;
}
