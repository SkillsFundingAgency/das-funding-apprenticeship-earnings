using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipEntityModel apprenticeshipEntityModel)
    {
        ApprenticeshipKey = apprenticeshipEntityModel.ApprenticeshipKey;
        ApprovalsApprenticeshipId = apprenticeshipEntityModel.ApprovalsApprenticeshipId;
        Uln = apprenticeshipEntityModel.Uln;

        ApprenticeshipEpisodes = apprenticeshipEntityModel.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisode(x)).ToList();
    }


    public Guid ApprenticeshipKey { get; }
    public long ApprovalsApprenticeshipId { get; }
    public string Uln { get; }

    public List<ApprenticeshipEpisode> ApprenticeshipEpisodes { get; }

    public void CalculateEarnings(ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.CalculateEarnings();
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarningsPriceChange(ISystemClockService systemClock, decimal newAgreedPrice, DateTime effectiveFromDate, List<Guid> deletedPriceKeys, Guid newPriceKey)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        if (currentEpisode.EarningsProfile == null)
            throw new Exception($"No earnings profile for current episode on {systemClock.UtcNow}");

        var existingEarnings = currentEpisode.EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        currentEpisode.UpdateAgreedPrice(systemClock, newAgreedPrice, deletedPriceKeys, newPriceKey);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarningsStartDateChange(ISystemClockService systemClock, DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship, List<Guid> deletedPriceKeys, Guid changingPriceKey)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateStartDate(newStartDate, newEndDate, ageAtStartOfApprenticeship, deletedPriceKeys, changingPriceKey);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(newStartDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }

    //todo remove this before merging into release
    public string Log107Data(ISystemClockService systemClock, List<Guid> deletedPriceKeys)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        var builder = new StringBuilder();
        builder.Append($"FLP-107 debug Prices: {JsonSerializer.Serialize(currentEpisode.Prices)}");
        builder.Append($" | FLP-107 debug deletedPriceKeys: {JsonSerializer.Serialize(deletedPriceKeys)}");
        return builder.ToString();
    }
}