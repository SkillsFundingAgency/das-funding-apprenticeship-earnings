namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEarningsQueryCommand;

public class UpdateEarningsQueryCommand : ICommand
{
    public UpdateEarningsQueryType UpdateType { get; }
    public Guid ApprenticeshipKey { get; }

    public UpdateEarningsQueryCommand(UpdateEarningsQueryType updateType, Guid apprenticeshipKey)
    {
        UpdateType = updateType;
        ApprenticeshipKey = apprenticeshipKey;
    }
}

public enum UpdateEarningsQueryType
{
    Create,
    Recalculate
}
