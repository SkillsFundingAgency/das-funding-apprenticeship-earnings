namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public interface IOnProgramTotalPriceCalculator
    {
        decimal CalculateOnProgramTotalPrice(decimal agreedPrice);
    }

    public class OnProgramTotalPriceCalculator : IOnProgramTotalPriceCalculator
    {
        private const decimal AgreedPriceMultiplier = 0.8m;
        public decimal CalculateOnProgramTotalPrice(decimal agreedPrice)
        {
            return agreedPrice * AgreedPriceMultiplier;
        }
    }
}
