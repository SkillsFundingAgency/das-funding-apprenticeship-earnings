using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.ServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public class LearningRepository : ILearningRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly ILearningFactory _learningFactory;
    private readonly IMessageSession _messageSession;

    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public LearningRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext, ILearningFactory learningFactory, IMessageSession messageSession)
    {
        _lazyContext = dbContext;
        _learningFactory = learningFactory;
        _messageSession = messageSession;
    }

    public async Task Add<TEntity, TEpisode>(BaseLearning<TEntity, TEpisode> learning) where TEntity : BaseLearningEntity where TEpisode : BaseEpisode
    {
        var entity = learning.GetModel();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(learning);
    }

    public async Task<ApprenticeshipLearning?> GetApprenticeshipLearning(Guid key)
    {
        var learning = await DbContext.ApprenticeshipLearnings
            .Include(x => x.Episodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.ApprenticeshipAdditionalPayments)
            .Include(x => x.Episodes)
                .ThenInclude(y => y.Prices)
            .Include(x => x.Episodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.EnglishAndMathsCourses)
                .ThenInclude(y => y.PeriodsInLearning)
            .Include(x => x.Episodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.EnglishAndMathsCourses)
                .ThenInclude(y => y.Instalments)
            .Include(x=> x.Episodes)
                .ThenInclude(y=> y.PeriodsInLearning)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.LearningKey == key);

        return learning == null ? null : _learningFactory.GetExistingApprenticeship(learning);
    }

    public async Task<ShortCourseLearning?> GetShortCourseLearning(Guid key)
    {
        var learning = await DbContext.ShortCourseLearnings
            .Include(x => x.Episodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.Instalments)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.LearningKey == key);

        return learning == null ? null : _learningFactory.GetExistingShortCourse(learning);
    }

    public async Task Update(ApprenticeshipLearning learning)
    {
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(learning);
    }

    public async Task Update(ShortCourseLearning learning)
    {
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(learning);
    }

    private async Task ReleaseEvents(AggregateRoot learning)
    {
        foreach (var @event in learning.FlushEvents())
        {
            await _messageSession.Publish(@event);
        }
    }
}
