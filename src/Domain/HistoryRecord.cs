namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public class HistoryRecord<T> where T : class
{
    public T Record { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}