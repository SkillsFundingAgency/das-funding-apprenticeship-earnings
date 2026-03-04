using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using LearningDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Learning;

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

    public async Task Add(LearningDomainModel learning)
    {
        var entity = learning.GetModel();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(learning);
    }

    public async Task<LearningDomainModel?> Get(Guid key)
    {
        var learning = await DbContext.Learnings
            .Include(x => x.ApprenticeshipEpisodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.Instalments)
            .Include(x => x.ApprenticeshipEpisodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.ApprenticeshipAdditionalPayments)
            .Include(x => x.ApprenticeshipEpisodes)
                .ThenInclude(y => y.Prices)
            .Include(x => x.ApprenticeshipEpisodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.EnglishAndMathsCourses)
                .ThenInclude(y => y.PeriodsInLearning)
            .Include(x => x.ApprenticeshipEpisodes)
                .ThenInclude(y => y.EarningsProfile)
                .ThenInclude(y => y.EnglishAndMathsCourses)
                .ThenInclude(y => y.Instalments)
            .Include(x=> x.ApprenticeshipEpisodes)
                .ThenInclude(y=> y.PeriodsInLearning)
            .Include(x => x.ShortCourseEpisodes)
                .ThenInclude(y=> y.EarningsProfile)
                .ThenInclude(y => y.Instalments)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.LearningKey == key);

        return learning == null ? null : _learningFactory.GetExisting(learning);
    }

    public async Task Update(LearningDomainModel learning)
    {
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(learning);
    }

    private async Task ReleaseEvents(LearningDomainModel learning)
    {
        foreach (var @event in learning.FlushEvents())
        {
            await _messageSession.Publish(@event); 
        }
    }
}