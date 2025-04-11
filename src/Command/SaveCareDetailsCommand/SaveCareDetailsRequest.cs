namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;

public class SaveCareDetailsRequest
{
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }
}
