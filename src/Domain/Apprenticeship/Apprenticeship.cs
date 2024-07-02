using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

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

    public void RecalculateEarnings(ISystemClockService systemClock, decimal newAgreedPrice, DateTime effectiveFromDate)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        var existingEarnings = currentEpisode.EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        currentEpisode.UpdateAgreedPrice(systemClock, newAgreedPrice);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClockService systemClock, DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateStartDate(newStartDate, newEndDate, ageAtStartOfApprenticeship);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(newStartDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }
}