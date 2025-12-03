namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveDateOfBirthCommand;

public class SaveDateOfBirthCommand : ICommand
{
    public SaveDateOfBirthCommand(Guid apprenticeshipKey, DateTime dateOfBirth)
    {
        ApprenticeshipKey = apprenticeshipKey;
        DateOfBirth = dateOfBirth;
    }

    public Guid ApprenticeshipKey { get; }

    public DateTime DateOfBirth { get; set; }
}
