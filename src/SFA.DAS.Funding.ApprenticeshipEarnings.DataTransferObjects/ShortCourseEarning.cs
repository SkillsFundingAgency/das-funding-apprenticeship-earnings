namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

public class ShortCourseEarning
{
    public short CollectionYear { get; set; }
    public byte CollectionPeriod { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
}
