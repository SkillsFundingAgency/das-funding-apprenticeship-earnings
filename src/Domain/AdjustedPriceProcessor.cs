namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public interface IAdjustedPriceProcessor
    {
        decimal CalculateAdjustedPrice(decimal agreedPrice);
    }

    public class AdjustedPriceProcessor : IAdjustedPriceProcessor
    {
        private const decimal AgreedPriceMultiplier = 0.8m;
        public decimal CalculateAdjustedPrice(decimal agreedPrice)
        {
            return agreedPrice * AgreedPriceMultiplier;
        }
    }
}
