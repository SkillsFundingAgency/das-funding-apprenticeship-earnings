using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;

public interface IFundingBandMaximumService
{
    Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate);
}

public class FundingBandMaximumService : IFundingBandMaximumService
{
    private readonly IEarningsOuterApiClient _earningsOuterApiClient;
    private readonly ILogger<FundingBandMaximumService> _logger;

    public FundingBandMaximumService(IEarningsOuterApiClient earningsOuterApiClient, ILogger<FundingBandMaximumService> logger)
    {
        _earningsOuterApiClient = earningsOuterApiClient;
        _logger = logger;
    }

    public async Task<int?> GetFundingBandMaximum(string courseCode, DateTime? startDate)
    {
        var standard = await _earningsOuterApiClient.GetStandard(courseCode);

        if (startDate == null)
            return null;

        return standard.ApprenticeshipFunding
            .SingleOrDefault(x => x.IsApplicableForDate(startDate.Value))?.MaxEmployerLevyCap;
    }
}