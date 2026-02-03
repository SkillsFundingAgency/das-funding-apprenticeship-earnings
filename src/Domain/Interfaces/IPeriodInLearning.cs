namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;

public interface IPeriodInLearning
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public DateTime OriginalExpectedEndDate { get; }
}
