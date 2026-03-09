using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;

public class GetShortCourseEarningsQueryHandler : IQueryHandler<GetShortCourseEarningsRequest, GetShortCourseEarningsResponse>
{
    private readonly ILearningRepository _learningRepository;
    private readonly ILogger<GetShortCourseEarningsQueryHandler> _logger;

    public GetShortCourseEarningsQueryHandler(ILearningRepository learningRepository, ILogger<GetShortCourseEarningsQueryHandler> logger)
    {
        _learningRepository = learningRepository;
        _logger = logger;
    }

    public async Task<GetShortCourseEarningsResponse> Handle(GetShortCourseEarningsRequest query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetShortCourseEarningsRequest for LearningKey: {learningKey} Ukprn: {ukprn}", query.LearningKey, query.Ukprn);

        var learning = await _learningRepository.GetShortCourseLearning(query.LearningKey);

        if (learning == null)
        {
            _logger.LogInformation("No short course learning found for LearningKey: {learningKey}", query.LearningKey);
            return new GetShortCourseEarningsResponse();
        }

        var episode = learning.Episodes.Single(e => e.UKPRN == query.Ukprn);

        if (episode.EarningsProfile == null)
        {
            _logger.LogInformation("No earnings profile found for LearningKey: {learningKey}", query.LearningKey);
            return new GetShortCourseEarningsResponse();
        }

        var earnings = episode.EarningsProfile.Instalments
            .Select(i => new GetShortCourseEarningsResponse.Earning
            {
                CollectionYear = i.AcademicYear,
                CollectionPeriod = i.DeliveryPeriod,
                Amount = i.Amount,
                Type = i.Type.ToString()
            })
            .ToList();

        return new GetShortCourseEarningsResponse { Earnings = earnings };
    }
}
