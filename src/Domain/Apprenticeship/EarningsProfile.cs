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

        public decimal AdjustedPrice { get; set; }
        public List<Instalment> Instalments { get; set; }
        public decimal CompletionPayment { get; set; }
    }
}
