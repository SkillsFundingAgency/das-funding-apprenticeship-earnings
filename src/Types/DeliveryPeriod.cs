namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class DeliveryPeriod
{
    public byte CalendarMonth { get; set; }
    public short CalenderYear { get; set; }
    public byte Period { get; set; }
    public short AcademicYear { get; set; }
    public decimal LearningAmount { get; set; }
    public string FundingLineType { get; set; }
    public string ReportingFundingLineType { get; set; }
}