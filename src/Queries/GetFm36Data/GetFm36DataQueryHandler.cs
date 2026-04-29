using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;

public class GetFm36DataQueryHandler : IQueryHandler<GetFm36DataRequest, GetFm36DataResponse>
{
    private readonly ApprenticeshipEarningsDataContext _dbContext;
    private readonly ILogger<GetFm36DataQueryHandler> _logger;

    public GetFm36DataQueryHandler(ApprenticeshipEarningsDataContext dbContext, ILogger<GetFm36DataQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GetFm36DataResponse> Handle(GetFm36DataRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetFm36DataRequest for Ukprn: {ukprn} Year:{collectionYear} Period:{collectionPeriod} LearningKeys:{learningKeys}", query.Ukprn, query.CollectionYear, query.CollectionPeriod, query.LearningKeysLogInfo());

        var searchDate = query.CollectionYear.ToDateTime(query.CollectionPeriod);
        var academicYearStart = searchDate.Month >= 8 ? new DateTime(searchDate.Year, 8, 1) : new DateTime(searchDate.Year - 1, 8, 1);
        var academicYearEnd = searchDate.Month >= 8 ? new DateTime(searchDate.Year + 1, 7, 31) : new DateTime(searchDate.Year, 7, 31);

        var dbQuery = _dbContext.ApprenticeshipLearnings
            .Where(x => x.Episodes.Any(e => e.Ukprn == query.Ukprn))
            .Where(x => x.Episodes.Any(e =>
                e.Prices.Any(p => p.StartDate <= academicYearEnd) &&
                !(e.WithdrawalDate.HasValue && e.WithdrawalDate.Value < academicYearStart) &&
                !(e.CompletionDate.HasValue && e.CompletionDate.Value < academicYearStart)))
            .Include(x => x.Episodes)
                .ThenInclude(x => x.Prices)
            .Include(x => x.Episodes)
                .ThenInclude(x => x.EarningsProfile)
                    .ThenInclude(x => x.Instalments)
            .Include(x => x.Episodes)
                .ThenInclude(x => x.EarningsProfile)
                    .ThenInclude(x => x.ApprenticeshipAdditionalPayments)
            .AsNoTracking()
            .AsSplitQuery();

        if (query.LearningKeys != null && query.LearningKeys.Any())
            dbQuery = dbQuery.Where(x => query.LearningKeys.Contains(x.LearningKey));

        var learnings = await dbQuery.ToListAsync(cancellationToken);

        var apprenticeships = learnings
            .Select(l => (learning: l, currentEpisode: GetCurrentEpisode(l.Episodes, searchDate)))
            .Where(x => x.currentEpisode?.Ukprn == query.Ukprn)
            .Select(x => MapApprenticeship(x.learning, x.currentEpisode!))
            .ToList();

        if (!apprenticeships.Any())
        {
            _logger.LogInformation("No apprenticeships found for: {ukprn}", query.Ukprn);
            return new GetFm36DataResponse();
        }

        return new GetFm36DataResponse { Apprenticeships = apprenticeships };
    }

    private static Apprenticeship MapApprenticeship(ApprenticeshipLearningEntity learning, ApprenticeshipEpisodeEntity currentEpisode)
    {
        var priceStartDate = currentEpisode.Prices.Min(p => p.StartDate);
        var ageAtStart = learning.DateOfBirth.CalculateAgeAtDate(priceStartDate);
        var fundingLineType = ageAtStart < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

        return new Apprenticeship
        {
            Key = learning.LearningKey,
            Ukprn = currentEpisode.Ukprn,
            FundingLineType = fundingLineType,
            Episodes = learning.Episodes.Select(MapEpisode).ToList()
        };
    }

    private static Episode MapEpisode(ApprenticeshipEpisodeEntity episode)
    {
        var profile = episode.EarningsProfile;

        return new Episode
        {
            Key = episode.Key,
            NumberOfInstalments = profile?.Instalments.Count ?? 0,
            Instalments = (profile?.Instalments ?? []).Select(i => new Instalment
            {
                AcademicYear = i.AcademicYear,
                DeliveryPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                EpisodePriceKey = i.EpisodePriceKey,
                InstalmentType = i.Type
            }).ToList(),
            AdditionalPayments = (profile?.ApprenticeshipAdditionalPayments ?? []).Select(p => new AdditionalPayment
            {
                AcademicYear = p.AcademicYear,
                DeliveryPeriod = p.DeliveryPeriod,
                Amount = p.Amount,
                AdditionalPaymentType = p.AdditionalPaymentType,
                DueDate = p.DueDate
            }).ToList(),
            CompletionPayment = profile?.CompletionPayment ?? 0,
            OnProgramTotal = profile?.OnProgramTotal ?? 0,
            EnglishAndMaths = (profile?.EnglishAndMathsCourses ?? []).Select(em => new EnglishAndMaths
            {
                LearnAimRef = em.LearnAimRef,
                StartDate = em.StartDate,
                EndDate = em.EndDate,
                Course = em.Course,
                Instalments = (em.Instalments ?? []).Select(i => new EnglishAndMathsInstalment
                {
                    AcademicYear = i.AcademicYear,
                    DeliveryPeriod = i.DeliveryPeriod,
                    Amount = i.Amount,
                    InstalmentType = i.Type
                }).ToList()

            }).ToList()
        };
    }

    private static ApprenticeshipEpisodeEntity? GetCurrentEpisode(List<ApprenticeshipEpisodeEntity> episodes, DateTime searchDate)
    {
        var episode = episodes.FirstOrDefault(e => e.Prices.Any(p => p.StartDate <= searchDate && p.EndDate >= searchDate));

        if (episode == null)
            episode = episodes.SingleOrDefault(e => e.Prices.Any(p => p.StartDate >= searchDate));

        if (episode == null)
            episode = episodes.Where(e => e.Prices.Any()).OrderByDescending(e => e.Prices.Max(p => p.EndDate)).FirstOrDefault();

        return episode;
    }
}
