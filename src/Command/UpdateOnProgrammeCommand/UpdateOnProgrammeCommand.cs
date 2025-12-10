
namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeCommand : ICommand
{
    public Guid ApprenticeshipKey { get; internal set; }
    public UpdateOnProgrammeRequest Request { get; set; }

    public UpdateOnProgrammeCommand(Guid apprenticeshipKey, UpdateOnProgrammeRequest request)
    {
        ApprenticeshipKey = apprenticeshipKey;
        Request = request;
    }
}
