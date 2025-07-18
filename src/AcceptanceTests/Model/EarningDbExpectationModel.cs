namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class EarningDbExpectationModel
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
    public string? Type { get; set; }
}