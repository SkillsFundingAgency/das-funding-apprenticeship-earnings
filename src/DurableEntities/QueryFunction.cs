using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.ContextImplementations;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Options;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Options;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models.GetApprenticeshipByUkprnResponse;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ApprenticeshipDomainExtensions = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.ApprenticeshipExtensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;

public class QueryFunction
{
    private readonly IDurableClientFactory _clientFactory;
    private readonly DurableClientOptions _options;
    private readonly ISystemClockService _systemClockService;

    public QueryFunction(IDurableClientFactory clientFactory,
        IOptions<DurableTaskOptions> options,
        ISystemClockService systemClockService)
    {
        _clientFactory = clientFactory;
        _options = new DurableClientOptions
        {
            TaskHub = options.Value.HubName
        };
        _systemClockService = systemClockService;
    }

    [FunctionName("QueryFunction")]
    public async Task<IActionResult> GetApprenticeshipEntites(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{ukprn}")] HttpRequest req,
        long ukprn)
    {
        var client = _clientFactory.CreateClient(_options);

        var query = new EntityQuery { PageSize = 100, FetchState = true };

        var entities = new List<Apprenticeship>();
        EntityQueryResult queryResult;

        do
        {
            queryResult = await client.ListEntitiesAsync(query, CancellationToken.None);

            var matchingEntities = ExtractMatchingEntities(queryResult, ukprn);
            entities.AddRange(matchingEntities);

            query.ContinuationToken = queryResult.ContinuationToken;

        } while (queryResult.ContinuationToken != null);

        return new OkObjectResult(entities);
    }

    private List<Apprenticeship> ExtractMatchingEntities(EntityQueryResult queryResult, long ukprn)
    {
        var entities = new List<Apprenticeship>();

        foreach (var entity in queryResult.Entities)
        {
            if (entity.State != null)
            {
                var apprenticeshipEntity = entity.State.ToObject<ApprenticeshipEntity>();
                var apprenticeship = new Domain.Apprenticeship.Apprenticeship(apprenticeshipEntity.Model);

                if (ApprenticeshipDomainExtensions.GetCurrentEpisode(apprenticeship, _systemClockService).UKPRN == ukprn)
                {
                    entities.Add(apprenticeship.ToApprenticeshipReponse(_systemClockService));
                }
            }
        }

        return entities;
    }
}
