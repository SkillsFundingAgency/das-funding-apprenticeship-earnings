using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;

public class GetFm99ShortCourseEarningsQueryHandler : IQueryHandler<GetFm99ShortCourseEarningsRequest, GetFm99ShortCourseEarningsResponse>
{
    private readonly ApprenticeshipEarningsDataContext _dbContext;
    private readonly ILogger<GetFm99ShortCourseEarningsQueryHandler> _logger;

    public GetFm99ShortCourseEarningsQueryHandler(ApprenticeshipEarningsDataContext dbContext, ILogger<GetFm99ShortCourseEarningsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GetFm99ShortCourseEarningsResponse> Handle(GetFm99ShortCourseEarningsRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetShortCourseEarningsRequest for LearningKey: {learningKey} Ukprn: {ukprn}", query.LearningKey, query.Ukprn);

        var earnings = await _dbContext.ShortCourseEpisodes
            .Where(e => e.LearningKey == query.LearningKey && e.Ukprn == query.Ukprn)
            .Where(e => e.EarningsProfile != null)
            .SelectMany(e => e.EarningsProfile!.Instalments)
            .Select(i => new GetFm99ShortCourseEarningsResponse.Earning
            {
                CollectionYear = i.AcademicYear,
                CollectionPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                Type = i.Type
            })
            .ToListAsync(cancellationToken);

        return new GetFm99ShortCourseEarningsResponse { Earnings = earnings };
    }
}
