using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

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

        // All conditions are evaluated against the same episode, and entirely in the database, so that paging is accurate
        var filteredQuery = _dbContext.ApprenticeshipLearnings
            .Where(x => x.Episodes.Any(e =>
                e.Ukprn == query.Ukprn &&
                !e.IsRemoved &&
                e.FundingPlatform == FundingPlatform.DAS &&

                // Include if started on or before end of the academic year
                e.Prices.Any(p => p.StartDate <= academicYearEnd) &&

                // Exclude if withdrawn before start of the academic year
                !(e.WithdrawalDate.HasValue && e.WithdrawalDate.Value < academicYearStart) &&

                // Exclude if completed before start of the academic year
                !(e.CompletionDate.HasValue && e.CompletionDate.Value < academicYearStart) &&

                // Exclude if withdrawn back to start
                !(e.WithdrawalDate.HasValue && e.WithdrawalDate.Value == e.Prices.Min(p => p.StartDate)) &&

                // With no actual end date recorded, the planned end date must not be before the start of the academic year
                (e.CompletionDate.HasValue || e.WithdrawalDate.HasValue || e.Prices.Any(p => p.EndDate >= academicYearStart))));

        if (query.LearningKeys != null && query.LearningKeys.Any())
            filteredQuery = filteredQuery.Where(x => query.LearningKeys.Contains(x.LearningKey));

        var totalItems = await filteredQuery.CountAsync(cancellationToken);

        IQueryable<ApprenticeshipLearningEntity> pagedQuery = filteredQuery.OrderBy(x => x.LearningKey);
        if (query.IsPaged)
            pagedQuery = pagedQuery.Skip(query.Offset).Take(query.PageSize!.Value);

        var learnings = await pagedQuery
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x => x.Prices)
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x => x.EarningsProfile)
                    .ThenInclude(x => x.Instalments)
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x => x.EarningsProfile)
                    .ThenInclude(x => x.ApprenticeshipAdditionalPayments)
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x=> x.EarningsProfile)
                    .ThenInclude(x => x.EnglishAndMathsCourses)
                        .ThenInclude(x => x.Instalments)
            .Include(x => x.Episodes.Where(e => !e.IsRemoved))
                .ThenInclude(x=> x.EarningsProfile)
                    .ThenInclude(x => x.EnglishAndMathsCourses)
                        .ThenInclude(x => x.AdditionalPayments)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // the current episode must also belong to this provider and be a DAS (opted-in) episode
        var apprenticeships = learnings
            .Select(l => (learning: l, currentEpisode: GetCurrentEpisode(l.Episodes, searchDate)))
            .Where(x => x.currentEpisode?.Ukprn == query.Ukprn && x.currentEpisode.FundingPlatform == FundingPlatform.DAS)
            .Select(x => MapApprenticeship(x.learning, x.currentEpisode!))
            .ToList();

        if (!apprenticeships.Any())
        {
            _logger.LogInformation("No apprenticeships found for: {ukprn}", query.Ukprn);
            return new GetFm36DataResponse { TotalItems = totalItems, Page = query.IsPaged ? query.Page : null, PageSize = query.PageSize };
        }

        return new GetFm36DataResponse
        {
            Apprenticeships = apprenticeships,
            TotalItems = totalItems,
            Page = query.IsPaged ? query.Page : null,
            PageSize = query.PageSize
        };
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
            AdditionalPayments = (profile?.ApprenticeshipAdditionalPayments ?? [])
                .Select(p => new AdditionalPayment
                {
                    AcademicYear = p.AcademicYear,
                    DeliveryPeriod = p.DeliveryPeriod,
                    Amount = p.Amount,
                    AdditionalPaymentType = p.AdditionalPaymentType,
                    DueDate = p.DueDate
                })
                .Concat((profile?.EnglishAndMathsCourses ?? []).SelectMany(em => em.AdditionalPayments ?? []).Select(p => new AdditionalPayment
                {
                    AcademicYear = p.AcademicYear,
                    DeliveryPeriod = p.DeliveryPeriod,
                    Amount = p.Amount,
                    AdditionalPaymentType = p.AdditionalPaymentType,
                    DueDate = p.DueDate
                }))
                .ToList(),
            CompletionPayment = profile?.CompletionPayment ?? 0,
            OnProgramTotal = profile?.OnProgramTotal ?? 0,
            FundingBandMaximum = episode.FundingBandMaximum,
            EnglishAndMaths = (profile?.EnglishAndMathsCourses ?? []).Where(em => !em.IsRemoved).Select(em => new EnglishAndMaths
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
