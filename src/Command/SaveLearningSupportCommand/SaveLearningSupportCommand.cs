namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;

public class SaveLearningSupportCommand : ICommand
{
    public SaveLearningSupportCommand(Guid learningKey, SaveLearningSupportRequest saveLearningSupportRequest)
    {
        LearningKey = learningKey;
        LearningSupportPayments = saveLearningSupportRequest;
    }

    public Guid LearningKey { get; }
    public List<LearningSupportPaymentDetail> LearningSupportPayments { get; set; } = new List<LearningSupportPaymentDetail>();
}
public class LearningSupportPaymentDetail
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
