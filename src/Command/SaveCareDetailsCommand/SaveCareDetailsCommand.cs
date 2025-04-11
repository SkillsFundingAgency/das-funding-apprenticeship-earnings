namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;

public class SaveCareDetailsCommand : ICommand
{
    public Guid ApprenticeshipKey { get; }
    public bool HasEHCP { get; }
    public bool IsCareLeaver { get; }
    public bool CareLeaverEmployerConsentGiven { get; }

    public SaveCareDetailsCommand(Guid apprenticeshipKey, SaveCareDetailsRequest saveCareDetailsRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        HasEHCP = saveCareDetailsRequest.HasEHCP;
        IsCareLeaver = saveCareDetailsRequest.IsCareLeaver;
        CareLeaverEmployerConsentGiven = saveCareDetailsRequest.CareLeaverEmployerConsentGiven;
    }
}
