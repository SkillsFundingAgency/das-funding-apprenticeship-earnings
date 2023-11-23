namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile
    {
        public EarningsProfile(decimal onProgramTotal, List<Instalment> instalments, decimal completionPayment, Guid earningsProfileId)
        {
            OnProgramTotal = onProgramTotal;
            Instalments = instalments;
            CompletionPayment = completionPayment;
            EarningsProfileId = earningsProfileId;
        }

        public Guid EarningsProfileId { get; }
        public decimal OnProgramTotal { get; }
        public List<Instalment> Instalments { get; }
        public decimal CompletionPayment { get; }
    }
}
