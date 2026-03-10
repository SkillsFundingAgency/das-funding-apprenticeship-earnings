using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;

public class GetShortCourseEarningsQueryHandler : IQueryHandler<GetShortCourseEarningsRequest, GetShortCourseEarningsResponse>
{
    private readonly ApprenticeshipEarningsDataContext _dbContext;
    private readonly ILogger<GetShortCourseEarningsQueryHandler> _logger;

    public GetShortCourseEarningsQueryHandler(ApprenticeshipEarningsDataContext dbContext, ILogger<GetShortCourseEarningsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GetShortCourseEarningsResponse> Handle(GetShortCourseEarningsRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetShortCourseEarningsRequest for LearningKey: {learningKey} Ukprn: {ukprn}", query.LearningKey, query.Ukprn);

        var earnings = await _dbContext.ShortCourseEpisodes
            .Where(e => e.LearningKey == query.LearningKey && e.Ukprn == query.Ukprn)
            .Where(e => e.EarningsProfile != null)
            .SelectMany(e => e.EarningsProfile!.Instalments)
            .Select(i => new GetShortCourseEarningsResponse.Earning
            {
                CollectionYear = i.AcademicYear,
                CollectionPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                Type = i.Type
            })
            .ToListAsync(cancellationToken);

        return new GetShortCourseEarningsResponse { Earnings = earnings };
    }
}
