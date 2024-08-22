using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile
    {
        private EarningsProfileModel _model;
        private List<Instalment> _instalments;

        public EarningsProfile(decimal onProgramTotal, List<Instalment> instalments, decimal completionPayment, Guid earningsProfileId)
        {
            _model.OnProgramTotal = onProgramTotal;
            _model.Instalments = instalments.Select(x => x.GetModel()).ToList();
            _instalments = instalments;
            _model.CompletionPayment = completionPayment;
            _model.EarningsProfileId = earningsProfileId;
        }

        public EarningsProfile(EarningsProfileModel model)
        {
            _model = model;
            _instalments = model.Instalments.Select(Instalment.Get).ToList();
        }

        public Guid EarningsProfileId { get; }
        public decimal OnProgramTotal { get; }
        public IReadOnlyCollection<Instalment> Instalments => new ReadOnlyCollection<Instalment>(_instalments);
        public decimal CompletionPayment { get; }

        public static EarningsProfile Get(EarningsProfileModel model)
        {
            return new EarningsProfile(model);
        }
    }
}