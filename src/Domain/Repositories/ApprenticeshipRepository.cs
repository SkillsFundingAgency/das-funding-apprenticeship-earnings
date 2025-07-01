using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
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
            .AsSplitQuery()
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
        foreach (var domainEvent in apprenticeship.FlushEvents())
        {
            await _messageSession.Publish(domainEvent); 
        }
    }

    public async Task Add(EarningsProfileHistoryModel earningsProfile)
    {
        DbContext.EarningsProfileHistories.Add(earningsProfile);
        await DbContext.SaveChangesAsync();
    }
}