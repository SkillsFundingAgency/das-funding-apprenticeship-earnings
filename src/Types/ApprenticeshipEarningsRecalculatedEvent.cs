﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types
{
#pragma warning disable CS8618
    public class ApprenticeshipEarningsRecalculatedEvent
    {
        public Guid ApprenticeshipKey { get; set; }
        public List<DeliveryPeriod> DeliveryPeriods { get; set; }
        public Guid EarningsProfileId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int AgeAtStartOfApprenticeship { get; set; }
    }
#pragma warning restore CS8618
}
