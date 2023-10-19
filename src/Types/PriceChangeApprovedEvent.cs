﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class PriceChangeApprovedEvent
{
    public Guid ApprenticeshipKey { get; set; }
    public int ApprenticeshipId { get; set; }

    public decimal TrainingPrice { get; set; }

    public decimal AssessmentPrice { get; set; }
    public DateTime EffectiveFromDate { get; set; }

    public DateTime ApprovedDate { get; set; }
    public ApprovedBy ApprovedBy { get; set; }

    public int EmployerAccountId { get; set; }

    public int ProviderId { get; set; }

}

public enum ApprovedBy
{
    Provider = 1,
    Employer = 2
}