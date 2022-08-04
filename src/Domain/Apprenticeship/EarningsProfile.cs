namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile
    {
        public EarningsProfile(decimal adjustedPrice, List<Instalment> instalments, decimal completionPayment)
        {
            AdjustedPrice = adjustedPrice;
            Instalments = instalments;
            CompletionPayment = completionPayment;
        }

        public decimal AdjustedPrice { get; }
        public List<Instalment> Instalments { get; }
        public decimal CompletionPayment { get; }
    }
}
