namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;

public class GetFm99ShortCourseEarningsResponse
{
    public List<Earning> Earnings { get; set; } = new();

    public class Earning
    {
        public short CollectionYear { get; set; }
        public byte CollectionPeriod { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
