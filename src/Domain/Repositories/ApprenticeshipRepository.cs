using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public class ApprenticeshipRepository : IApprenticeshipRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly IApprenticeshipFactory _apprenticeshipFactory;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public ApprenticeshipRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext, IApprenticeshipFactory apprenticeshipFactory, IDomainEventDispatcher domainEventDispatcher)
    {
        _lazyContext = dbContext;
        _apprenticeshipFactory = apprenticeshipFactory;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task Add(Apprenticeship.Apprenticeship apprenticeship)
    {
        var entity = apprenticeship.GetModel();
        await DbContext.AddAsync(entity);
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(apprenticeship);
    }

    public async Task<Apprenticeship.Apprenticeship> Get(Guid key)
    {
        var apprenticeship = await DbContext.Apprenticeships
            .AsTracking()
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfileHistory)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .SingleAsync(x => x.Key == key);

        return _apprenticeshipFactory.GetExisting(apprenticeship);
    }

    public async Task Update(Apprenticeship.Apprenticeship apprenticeship)
    {
        var entity = apprenticeship.GetModel();
        foreach (var episode in entity.Episodes)
        {
            await DbContext.EarningsProfiles.AddAsync(episode.EarningsProfile);
            foreach (var instalment in episode.EarningsProfile.Instalments)
            {
                await DbContext.Instalments.AddAsync(instalment);
            }
            foreach (var earningsProfileHistory in episode.EarningsProfileHistory)
            {
                await DbContext.EarningsProfileHistories.AddAsync(earningsProfileHistory);
                foreach (var instalment in earningsProfileHistory.Instalments)
                {
                    await DbContext.InstalmentHistories.AddAsync(instalment);
                }
            }
        }
        await DbContext.SaveChangesAsync();
        await ReleaseEvents(apprenticeship);
    }

    private async Task ReleaseEvents(Apprenticeship.Apprenticeship apprenticeship)
    {
        foreach (dynamic domainEvent in apprenticeship.FlushEvents())
        {
            await _domainEventDispatcher.Send(domainEvent);
        }
    }
}