namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;

public class SaveCareDetailsCommand : ICommand
{
    public Guid LearningKey { get; }
    public bool HasEHCP { get; }
    public bool IsCareLeaver { get; }
    public bool CareLeaverEmployerConsentGiven { get; }

    public SaveCareDetailsCommand(Guid LearningKey, SaveCareDetailsRequest saveCareDetailsRequest)
    {
        LearningKey = LearningKey;
        HasEHCP = saveCareDetailsRequest.HasEHCP;
        IsCareLeaver = saveCareDetailsRequest.IsCareLeaver;
        CareLeaverEmployerConsentGiven = saveCareDetailsRequest.CareLeaverEmployerConsentGiven;
    }
}
