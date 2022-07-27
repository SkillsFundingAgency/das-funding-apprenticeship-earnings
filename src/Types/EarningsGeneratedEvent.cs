using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class EarningsGeneratedEvent : IEvent
{
    public Guid ApprenticeshipKey { get; set; }
    public List<FundingPeriod> FundingPeriods { get; set; }
}

public class FundingPeriod
{
    public long Uln { get; set; }
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
}

public class DeliveryPeriod
{
    public byte CalendarMonth { get; set; }
    public short CalenderYear { get; set; }
    public byte Period { get; set; }
    public short AcademicYear { get; set; }
    public decimal LearningAmount { get; set; }
    public string FundingLineType { get; set; }
    public string ReportingFundingLineType { get; set; }
}

public enum EmployerType
{
    Levy,
    NonLevy
}

public enum EmploymentStatus
{
    Employed,
    Redundant
}

public enum TrainingStatus
{
    InLearning,
    Break
}