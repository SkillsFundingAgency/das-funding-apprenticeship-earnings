namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;

public class SaveLearningSupportCommand : ICommand
{
    public SaveLearningSupportCommand(Guid apprenticeshipKey, SaveLearningSupportRequest saveLearningSupportRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        LearningSupportPayments = saveLearningSupportRequest;
    }

    public Guid ApprenticeshipKey { get; }
    public List<LearningSupportPaymentDetail> LearningSupportPayments { get; set; } = new List<LearningSupportPaymentDetail>();
}
public class LearningSupportPaymentDetail
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
