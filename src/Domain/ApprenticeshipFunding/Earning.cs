namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding
{
    public class Earning
    {
        public short AcademicYear { get; set; }
        public byte DeliveryPeriod { get; set; }
        public decimal Amount { get; set; }
    }
}
