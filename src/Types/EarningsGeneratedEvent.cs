namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;
#pragma warning disable CS8618

public class EarningsGeneratedEvent
{
    public Guid ApprenticeshipKey { get; set; }
    public string Uln { get; set; }
    public long CommitmentId { get; set; }
    public long EmployerId { get; set; }
    public long ProviderId { get; set; }
    public long? TransferSenderEmployerId { get; set; }
    public decimal AgreedPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ActualEndDate { get; set; }
    public string TrainingCode { get; set; }
    public EmployerType EmployerType { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public TrainingStatus TrainingStatus { get; set; }
    public decimal CoInvestmentPercentage { get; set; }
    public List<DeliveryPeriod> DeliveryPeriods { get; set; }
    public long EmployerAccountId { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public Guid EarningsProfileId { get; set; }
}
#pragma warning restore CS8618
