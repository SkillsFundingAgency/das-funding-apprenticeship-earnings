namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Instalment
    {
        public Instalment(short academicYear, byte deliveryPeriod, decimal amount)
        {
            AcademicYear = academicYear;
            DeliveryPeriod = deliveryPeriod;
            Amount = amount;
        }

        public short AcademicYear { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
    }
}
