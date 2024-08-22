namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public class HistoryRecord<T> where T : class
{
    public HistoryRecord(T record, DateTime supersededDate)
    {
        Record = record;
        SupersededDate = supersededDate;
    }

    public T Record { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}