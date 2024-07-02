using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

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

        public EarningsProfile(EarningsProfileEntityModel model)
        {
            var instalments = model.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
            OnProgramTotal = model.AdjustedPrice;
            Instalments = instalments;
            CompletionPayment = model.CompletionPayment;
            EarningsProfileId = model.EarningsProfileId;
        }

        public Guid EarningsProfileId { get; }
        public decimal OnProgramTotal { get; }
        public List<Instalment> Instalments { get; }
        public decimal CompletionPayment { get; }
    }
}
