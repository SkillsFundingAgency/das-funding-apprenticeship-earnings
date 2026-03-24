using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;

public class GetShortCourseQueryHandler : IQueryHandler<GetShortCourseRequest, GetShortCourseResponse>
{
    private readonly ApprenticeshipEarningsDataContext _dbContext;
    private readonly ILogger<GetShortCourseQueryHandler> _logger;

    public GetShortCourseQueryHandler(ApprenticeshipEarningsDataContext dbContext, ILogger<GetShortCourseQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GetShortCourseResponse> Handle(GetShortCourseRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetShortCourseEarningsRequest for LearningKey: {learningKey} Ukprn: {ukprn}", query.LearningKey, query.Ukprn);

        var result = await _dbContext.ShortCourseEpisodes
            .AsNoTracking()
            .Where(e => e.LearningKey == query.LearningKey && e.Ukprn == query.Ukprn)
            .Select(episode => new
            {
                episode.EarningsProfile.Version,
                episode.Milestones,
                Instalments = episode.EarningsProfile.Instalments.Select(instalment => new
                {
                    instalment.AcademicYear,
                    instalment.DeliveryPeriod,
                    instalment.Amount,
                    instalment.Type
                }).ToList()
            })
            .SingleAsync(cancellationToken);

        var instalments = result.Instalments.Select(i => new ShortCourseInstalment
        {
            CollectionYear = i.AcademicYear,
            CollectionPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            Type = i.Type,
            IsPayable = IsInstalmentPayable(i.Type, result.Milestones)
        }).ToList();

        return new GetShortCourseResponse
        {
            EarningProfileVersion = result.Version,
            Instalments = instalments
        };
    }

    private bool IsInstalmentPayable(string type, MilestoneFlags milestones)
    {
        var flagEnum = Enum.Parse<MilestoneFlags>(type);
        return milestones.HasFlag(flagEnum);
    }
}