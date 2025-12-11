namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UpdateOnProgrammeModel
{
    // Price changes
    public DateTime? PriceStartDate { get; set; }
    public DateTime? PriceEndDate{ get; set; }
    public decimal? NewTrainingPrice { get; set; }
    public decimal? NewAssessmentPrice { get; set; }

    // Date of birth change
    public DateTime? DateOfBirth { get; set; }
}