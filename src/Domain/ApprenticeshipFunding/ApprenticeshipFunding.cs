namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding
{
    public class ApprenticeshipFunding
    {
        private const decimal AgreedPriceMultiplier = 0.8m;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        public decimal AdjustedPrice { get; }
        public decimal OnProgramTotal => CalculateOnProgramTotal();
        public decimal CompletionPayment => CalculateCompletionPayment();

        public ApprenticeshipFunding(decimal agreedPrice, DateTime startDate, DateTime endDate, decimal fundingBandMaximum)
        {
            _startDate = startDate;
            _endDate = endDate;
            AdjustedPrice = CalculateAdjustedPrice(agreedPrice, fundingBandMaximum);
        }

        private decimal CalculateAdjustedPrice(decimal agreedPrice, decimal fundingBandMaximum)
        {
            return Math.Min(agreedPrice, fundingBandMaximum);
        }

        private decimal CalculateCompletionPayment()
        {
            return AdjustedPrice - OnProgramTotal;
        }

        private decimal CalculateOnProgramTotal()
        {
            return AdjustedPrice * AgreedPriceMultiplier;
        }

        public List<Earning> GenerateEarnings()
        {
            var instalmentGenerator = new InstalmentsGenerator();
            var earnings = instalmentGenerator.Generate(OnProgramTotal, _startDate, _endDate);
            return earnings;
        }
    }
}
