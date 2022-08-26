using Microsoft.Azure.WebJobs;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Functions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions
{
    public static class JobHostExtensions
    {
        public static async Task<IJobHost> GetEntity(this IJobHost jobs, string entityType, string entityKey)
        {
            await jobs.CallAsync(nameof(GetEntityFunction), new Dictionary<string, object>
            {
                ["entityType"] = entityType,
                ["entityKey"] = entityKey,
                ["timeout"] = new TimeSpan(0, 0, 1, 0)
            });

            return jobs;
        }
    }
}
