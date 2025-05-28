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

        public EarningsProfile(
            decimal onProgramTotal,
            List<Instalment> instalments,
            List<AdditionalPayment> additionalPayments,
            List<MathsAndEnglish> mathsAndEnglishCourses,
            decimal completionPayment,
            Guid episodeKey)
        {
            Model = new EarningsProfileModel();
            Model.EarningsProfileId = Guid.NewGuid();
            Model.OnProgramTotal = onProgramTotal;
            Model.Instalments = instalments.Select(x => x.GetModel(Model.EarningsProfileId)).ToList();
            Model.AdditionalPayments = additionalPayments.Select(x => x.GetModel(Model.EarningsProfileId)).ToList();
            Model.MathsAndEnglishCourses = mathsAndEnglishCourses.Select(x => x.GetModel(Model.EarningsProfileId)).ToList();
            _instalments = instalments;
            Model.CompletionPayment = completionPayment;
            Model.EpisodeKey = episodeKey;
        }

        public EarningsProfile(EarningsProfileModel model)
        {
            Model = model;
            _instalments = model.Instalments?.Select(Instalment.Get).ToList();
        }

        public Guid EarningsProfileId => Model.EarningsProfileId;
        public decimal OnProgramTotal => Model.OnProgramTotal;
        public IReadOnlyCollection<Instalment> Instalments => new ReadOnlyCollection<Instalment>(_instalments);
        public IReadOnlyCollection<AdditionalPayment> AdditionalPayments => Model.AdditionalPayments.Select((AdditionalPayment.Get)).ToList().AsReadOnly();
        public IReadOnlyCollection<MathsAndEnglish> MathsAndEnglishCourses => Model.MathsAndEnglishCourses.Select((MathsAndEnglish.Get)).ToList().AsReadOnly();
        public decimal CompletionPayment => Model.CompletionPayment;

        /// <summary>
        /// Some payments are not calculated, but are instead added to the earnings profile via an external process.
        /// In the event of a recalculation, these payments should be preserved.
        /// This method returns the list of payments that are not calculated.
        /// </summary>
        public IReadOnlyCollection<AdditionalPayment> PersistentAdditionalPayments()
        {
            return Model.AdditionalPayments
                .Where(x=> x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
                .Select((AdditionalPayment.Get))
                .ToList().AsReadOnly();
        }

        /// <summary>
        /// Maths and English courses are not calculated, but are instead added to the earnings profile via an external process.
        /// In the event of a recalculation, these should be preserved.
        /// This method returns the courses so that they can be preserved.
        /// </summary>
        public IReadOnlyCollection<MathsAndEnglish> PersistentMathsAndEnglishCourses()
        {
            return Model.MathsAndEnglishCourses
                .Select((MathsAndEnglish.Get))
                .ToList().AsReadOnly();
        }

        public static EarningsProfile Get(EarningsProfileModel model)
        {
            return new EarningsProfile(model);
        }
    }
}