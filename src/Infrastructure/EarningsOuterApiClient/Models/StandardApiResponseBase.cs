using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient.Models;

#pragma warning disable CS8618
[ExcludeFromCodeCoverage]
public abstract class StandardApiResponseBase
{
    public StandardDate StandardDates { get; set; }
    public List<ApprenticeshipFunding> ApprenticeshipFunding { get; set; }

    [JsonIgnore]
    public int MaxFunding => GetFundingDetails(nameof(MaxFunding));
    [JsonIgnore]
    public int TypicalDuration => GetFundingDetails(nameof(TypicalDuration));
    [JsonIgnore]
    public bool IsActive => this.IsStandardActive();
    public int MaxFundingOn(DateTime effectiveDate) => GetFundingDetails(nameof(MaxFunding), effectiveDate);
    protected virtual int GetFundingDetails(string prop, DateTime? effectiveDate = null) => this.FundingDetails(prop, effectiveDate);
}

[ExcludeFromCodeCoverage]
public class StandardDate
{
    public DateTime? LastDateStarts { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime EffectiveFrom { get; set; }
}

public static class StandardApiResponseBaseExtensions
{
    public static int FundingDetails(this StandardApiResponseBase standardApiResponseBase, string prop, DateTime? effectiveDate = null)
    {
        if (standardApiResponseBase.ApprenticeshipFunding == null || !standardApiResponseBase.ApprenticeshipFunding.Any()) return 0;

        var effDate = effectiveDate ?? DateTime.UtcNow;

        var funding = standardApiResponseBase.ApprenticeshipFunding
            .FirstOrDefault(c =>
                c.EffectiveFrom <= effDate
                && (c.EffectiveTo == null || c.EffectiveTo >= effDate));

        if (funding == null)
        {
            funding = standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault(c => c.EffectiveTo == null);
        }

        if (prop == nameof(standardApiResponseBase.MaxFunding))
        {
            return funding?.MaxEmployerLevyCap
                   ?? standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault()?.MaxEmployerLevyCap
                   ?? 0;
        }

        return funding?.Duration
               ?? standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault()?.Duration
               ?? 0;
    }

    public static bool IsStandardActive(this StandardApiResponseBase standardApiResponseBase)
    {
        if (standardApiResponseBase.StandardDates == null) return false;

        return standardApiResponseBase.StandardDates.EffectiveFrom.Date <= DateTime.UtcNow.Date
               && (!standardApiResponseBase.StandardDates.EffectiveTo.HasValue ||
                   standardApiResponseBase.StandardDates.EffectiveTo.Value.Date >= DateTime.UtcNow.Date)
               && (!standardApiResponseBase.StandardDates.LastDateStarts.HasValue ||
                   standardApiResponseBase.StandardDates.LastDateStarts.Value.Date >= DateTime.UtcNow.Date);
    }
}
#pragma warning restore CS8618
