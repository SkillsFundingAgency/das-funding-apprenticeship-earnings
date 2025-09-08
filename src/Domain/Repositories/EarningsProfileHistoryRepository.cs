using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsProfileHistoryRepository
{
    Task Add(EarningsProfileHistory item);
}
public class EarningsProfileHistoryRepository(Lazy<ApprenticeshipEarningsDataContext> lazyContext) : IEarningsProfileHistoryRepository
{
    private ApprenticeshipEarningsDataContext DbContext => lazyContext.Value;

    public async Task Add(EarningsProfileHistory item)
    {
        await DbContext.EarningsProfileHistories2.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }
}