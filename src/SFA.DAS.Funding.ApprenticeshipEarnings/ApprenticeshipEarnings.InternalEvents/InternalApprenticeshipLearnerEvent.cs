using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents
{
    //todo this obviously needs to come from the event that will be published by the upstream service (Apprenticeship Earnings Approvals Event Handlers or similar)
    [Obsolete("this obviously needs to come from the event that will be published by the upstream service (Apprenticeship Earnings Approvals Event Handlers or similar)")]
    public class InternalApprenticeshipLearnerEvent : IEvent
    {
        public string ApprenticeshipKey { get; set; }
        public long CommitmentId { get; set; }
        public DateTime ApprovedOn { get; set; }
        public DateTime AgreedOn { get; set; }
        public long Uln { get; set; }
        public long ProviderId { get; set; }
        public long EmployerId { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal AgreedPrice { get; set; }
        public string TrainingCode { get; set; }
        public long? TransferSenderEmployerId { get; set; }
        public EmployerType EmployerType { get; set; }
    }

    public enum EmployerType
    {
        Levy,
        NonLevy
    }
}