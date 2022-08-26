namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile
    {
        public EarningsProfile(decimal onProgramTotal, List<Instalment> instalments, decimal completionPayment)
        {
            OnProgramTotal = onProgramTotal;
            Instalments = instalments;
            CompletionPayment = completionPayment;
        }

        public decimal OnProgramTotal { get; }
        public List<Instalment> Instalments { get; }
        public decimal CompletionPayment { get; }
    }
}
