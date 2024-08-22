using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class EarningsProfile<T> where T : EarningsProfileModel, new()
    {
        protected T Model;

        public EarningsProfile() { }

        public EarningsProfile(T model)
        {
            Model = model;
        }

        public T GetModel()
        {
            return Model;
        }
    }

    public class EarningsProfile : EarningsProfile<EarningsProfileModel>
    {
        private List<Instalment> _instalments;

        public EarningsProfile(decimal onProgramTotal, List<Instalment> instalments, decimal completionPayment, Guid earningsProfileId)
        {
            Model = new EarningsProfileModel();
            Model.OnProgramTotal = onProgramTotal;
            Model.Instalments = instalments.Select(x => x.GetModel()).ToList();
            _instalments = instalments;
            Model.CompletionPayment = completionPayment;
            Model.EarningsProfileId = earningsProfileId;
        }

        public EarningsProfile(EarningsProfileModel model)
        {
            Model = model;
            _instalments = model.Instalments?.Select(Instalment.Get).ToList();
        }

        public Guid EarningsProfileId => Model.EarningsProfileId;
        public decimal OnProgramTotal => Model.OnProgramTotal;
        public IReadOnlyCollection<Instalment> Instalments => new ReadOnlyCollection<Instalment>(_instalments);
        public decimal CompletionPayment => Model.CompletionPayment;

        public static EarningsProfile Get(EarningsProfileModel model)
        {
            return new EarningsProfile(model);
        }
    }
}