namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding
{
    public class ApprenticeshipFunding
    {
        private const decimal AgreedPriceMultiplier = 0.8m;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        public decimal AdjustedPrice { get; }
        public decimal CompletionPayment { get; }

        public ApprenticeshipFunding(decimal agreedPrice, DateTime startDate, DateTime endDate)
        {
            _startDate = startDate;
            _endDate = endDate;
            AdjustedPrice = CalculateAdjustedPrice(agreedPrice);
            CompletionPayment = CalculateCompletionPayment(agreedPrice);
        }

        private decimal CalculateCompletionPayment(decimal agreedPrice)
        {
            return agreedPrice - AdjustedPrice;
        }

        private decimal CalculateAdjustedPrice(decimal agreedPrice)
        {
            return agreedPrice * AgreedPriceMultiplier;
        }

        public List<Earning> GenerateEarnings()
        {
            var instalmentGenerator = new InstalmentsGenerator();
            var earnings = instalmentGenerator.Generate(AdjustedPrice, _startDate, _endDate);
            return earnings;
        }
    }
}
