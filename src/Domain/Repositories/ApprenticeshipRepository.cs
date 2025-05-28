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
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfileHistory)
            .ThenInclude(y => y.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.AdditionalPayments)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.Prices)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.MathsAndEnglishCourses)
            .Include(x => x.Episodes)
            .ThenInclude(y => y.EarningsProfile)
            .ThenInclude(y => y.MathsAndEnglishCourses)
            .ThenInclude(y => y.Instalments)
            .SingleAsync(x => x.Key == key);

        return _apprenticeshipFactory.GetExisting(apprenticeship);
    }

    public async Task Update(Apprenticeship.Apprenticeship apprenticeship)
    {
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