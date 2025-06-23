using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;

public class EarningsProfileArchivedEvent : IFireAndForgetDomainEvent
{
    public EarningsProfileHistoryModel EarningsProfileHistoryModel { get; }
    public EarningsProfileArchivedEvent(EarningsProfileHistoryModel earningsProfileHistoryModel)
    {
        EarningsProfileHistoryModel = earningsProfileHistoryModel;
    }
}
