using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsProfileHistoryRepository
{
    Task Add(ApprenticeshipEarningsProfileHistoryEntity item);
    Task Add(ShortCourseEarningsProfileHistoryEntity item);
}
public class EarningsProfileHistoryRepository(Lazy<ApprenticeshipEarningsDataContext> lazyContext) : IEarningsProfileHistoryRepository
{
    private ApprenticeshipEarningsDataContext DbContext => lazyContext.Value;

    public async Task Add(ApprenticeshipEarningsProfileHistoryEntity item)
    {
        await DbContext.EarningsProfileHistories2.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }

    public async Task Add(ShortCourseEarningsProfileHistoryEntity item)
    {
        await DbContext.ShortCourseEarningsProfileHistories.AddAsync(item);
        await DbContext.SaveChangesAsync();
    }
}