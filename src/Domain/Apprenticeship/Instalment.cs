using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Instalment
    {
        private InstalmentModel _model;

        public Instalment(short academicYear, byte deliveryPeriod, decimal amount)
        {
            _model = new InstalmentModel
            {
                AcademicYear = academicYear,
                DeliveryPeriod = deliveryPeriod,
                Amount = amount,
            };
        }

        private Instalment(InstalmentModel model)
        {
            _model = model;
        }

        public short AcademicYear { get; }
        public byte DeliveryPeriod { get; }
        public decimal Amount { get; }

        public InstalmentModel GetModel()
        {
            return _model;
        }

        public static Instalment Get(InstalmentModel model)
        {
            return new Instalment(model);
        }
    }
}
