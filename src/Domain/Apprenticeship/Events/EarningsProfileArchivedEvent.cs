using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsProfileArchivedEvent : IDomainEvent
{
    public EarningsProfileHistoryModel EarningsProfileHistoryModel { get; }
    public EarningsProfileArchivedEvent(EarningsProfileHistoryModel earningsProfileHistoryModel)
    {
        EarningsProfileHistoryModel = earningsProfileHistoryModel;
    }
}
