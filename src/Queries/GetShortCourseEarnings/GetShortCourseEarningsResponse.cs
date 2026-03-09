namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;

public class GetShortCourseEarningsResponse
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
