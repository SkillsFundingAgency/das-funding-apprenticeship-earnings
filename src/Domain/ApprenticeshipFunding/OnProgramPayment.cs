namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class OnProgramPayment
{
    public Guid PriceKey { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}
