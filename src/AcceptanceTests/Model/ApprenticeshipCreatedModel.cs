namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model
{
    public class PriceEpisodeSetupModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Price { get; set; }
        public int? FundingBandMaximum { get; set; }
    }

    public class ApprenticeshipCreatedSetupModel
    {
        public int? Age { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Price { get; set; }
    }
}
