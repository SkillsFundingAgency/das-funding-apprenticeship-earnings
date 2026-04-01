namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

public class ShortCourseEarnings
{
    public Guid EarningProfileVersion { get; set; }
    public List<ShortCourseInstalment> Instalments { get; set; } = new();
}

public class ShortCourseInstalment
{
    public short CollectionYear { get; set; }
    public byte CollectionPeriod { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsPayable { get; set; }
}
