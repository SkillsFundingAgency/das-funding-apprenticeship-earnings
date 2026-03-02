using Microsoft.EntityFrameworkCore;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public class EarningsQueryRepository : IEarningsQueryRepository
{
    private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
    private readonly ISystemClockService _systemClockService;
    private readonly IAcademicYearService _academicYearService;

    private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

    public EarningsQueryRepository(
        Lazy<ApprenticeshipEarningsDataContext> dbContext, 
        ISystemClockService systemClockService, 
        IAcademicYearService academicYearService)
    {
        _lazyContext = dbContext;
        _systemClockService = systemClockService;
        _academicYearService = academicYearService;
    }

    public List<Apprenticeship.Apprenticeship> GetApprenticeships(List<Guid>? learningKeys, long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false)
    {
        var query = GetApprenticeshipsQuery(ukprn, searchDate, onlyActiveApprenticeships);
        if (learningKeys != null && learningKeys.Any())
            query = query.Where(x => learningKeys.Contains(x.LearningKey));

        var apprenticeships = query
            .AsNoTracking()
            .AsSplitQuery()
            .ToList()
            .Select(z => Apprenticeship.Apprenticeship.Get(z));

        if (apprenticeships == null || !apprenticeships.Any())
            return new List<Apprenticeship.Apprenticeship>();

        // now get apprenticeships which currently belong to the ukprn
        return apprenticeships.Where(x => x.GetCurrentEpisode(searchDate).UKPRN == ukprn).ToList();
    }

    /// <summary>
    /// Retrieves a list of apprenticeships for a specific provider.
    /// </summary>
    /// <param name="ukprn">The unique provider reference number. Only apprenticeships where the episode (matching the searchDate) with this provider reference will be returned.</param>
    /// <param name="searchDate">The date to search for. Apprenticeships may belong to different providers at different times, so this date determines when the provider match is valid.</param>
    /// <param name="onlyActiveApprenticeships">If true, only apprenticeships that are currently active (i.e., have started and not finished) will be included.</param>
    /// <returns>A list of apprenticeships for the specified provider, or null if no matching apprenticeships are found.</returns>
    public List<Apprenticeship.Apprenticeship>? GetApprenticeships(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false)
    {
        var query = GetApprenticeshipsQuery(ukprn, searchDate, onlyActiveApprenticeships);

        var apprenticeships = query
            .Select(z => Apprenticeship.Apprenticeship.Get(z))
            .ToList();

        if (apprenticeships == null || !apprenticeships.Any())
            return null;

        // now get apprenticeships which currently belong to the ukprn
        var currentApprenticeships = apprenticeships.Where(x => x.GetCurrentEpisode(searchDate).UKPRN == ukprn).ToList();

        return currentApprenticeships;
    }

    private IQueryable<DataAccess.Entities.LearningModel> GetApprenticeshipsQuery(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false)
    {
        // first get any apprenticeships which belonged to the ukprn, splitting this query will improve performance
        IQueryable<DataAccess.Entities.LearningModel> query = DbContext.Learnings
            .Where(x => x.Episodes.Any(y => y.Ukprn == ukprn))
            .Include(x => x.Episodes)
            .ThenInclude(x => x.Prices)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.EarningsProfile)
            .ThenInclude(x => x.Instalments)
            .Include(x => x.Episodes)
            .ThenInclude(x => x.EarningsProfile)
            .ThenInclude(x => x.AdditionalPayments);

        if (onlyActiveApprenticeships)
        {
            var startDate = _academicYearService.StartOfCurrentAcademicYear(searchDate);
            var endDate = _academicYearService.EndOfCurrentAcademicYear(searchDate);
            query = query.Where(x => x.Episodes.Any(y =>
                y.Prices.Any(price => price.EndDate >= startDate) && // end date is at least after the start of this academic year
                y.Prices.Any(price => price.StartDate <= endDate)));  // start date is at least before the end of this academic year
        }

        return query;
    }


}
