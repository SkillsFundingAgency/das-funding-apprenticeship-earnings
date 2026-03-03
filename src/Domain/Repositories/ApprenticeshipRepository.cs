using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public class ApprenticeshipRepository : IApprenticeshipRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly IApprenticeshipFactory _apprenticeshipFactory;
    private readonly IMessageSession _messageSession;

    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public ApprenticeshipRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext, IApprenticeshipFactory apprenticeshipFactory, IMessageSession messageSession)
    {
        _lazyContext = dbContext;
        _apprenticeshipFactory = apprenticeshipFactory;
        _messageSession = messageSession;
    }

    public async Task Add(Models.Learning apprenticeship)
    {
        var entity = apprenticeship.GetModel();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(apprenticeship);
    }

    public async Task<Models.Learning?> Get(Guid key)
    {
        var apprenticeship = await DbContext.Learnings
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
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.LearningKey == key);

        return apprenticeship == null ? null : _apprenticeshipFactory.GetExisting(apprenticeship);
    }

    public async Task Update(Models.Learning apprenticeship)
    {
        try
        {
            await DbContext.SaveChangesAsync();
            await ReleaseEvents(apprenticeship);
        }
        catch (Exception ex)
        {
            var foo = ex;
        }

    }

    private async Task ReleaseEvents(Models.Learning apprenticeship)
    {
        foreach (var @event in apprenticeship.FlushEvents())
        {
            await _messageSession.Publish(@event); 
        }
    }
}