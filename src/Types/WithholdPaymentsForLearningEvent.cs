using SFA.DAS.Payments.EarningEvents.Messages.External;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

/// <summary>
/// Signal to withhold all payments for a learning, because a change to it is waiting for further approval.
/// Withholding ends when the earnings are next released.
/// </summary>
public class WithholdPaymentsForLearningEvent
{
    public long UKPRN { get; set; }
    public long ULN { get; set; }
    public Guid LearningKey { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public LearningType LearningType { get; set; }
    public CourseType CourseType { get; set; }
}
