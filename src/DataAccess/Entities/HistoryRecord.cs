namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public class HistoryRecord<T> where T : class
{
    public HistoryRecord() { }
    public HistoryRecord(T record, DateTime supersededDate)
    {
        Record = record;
        SupersededDate = supersededDate;
    }

    [Key]
    public Guid Key { get; set; }
    public T Record { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}