using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class PurgeBackgroundJob : BackgroundService
{
    private readonly IJobHost _jobHost;
    public PurgeBackgroundJob(IJobHost jobHost)
    {
        _jobHost = jobHost;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _jobHost.Purge();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}