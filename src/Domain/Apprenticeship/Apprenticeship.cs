using Microsoft.Extensions.Internal;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipEntityModel apprenticeshipEntityModel)
    {
        ApprenticeshipKey = apprenticeshipEntityModel.ApprenticeshipKey;
        ApprovalsApprenticeshipId = apprenticeshipEntityModel.ApprovalsApprenticeshipId;
        Uln = apprenticeshipEntityModel.Uln;
        FundingEmployerAccountId = apprenticeshipEntityModel.FundingEmployerAccountId;

        ApprenticeshipEpisodes = apprenticeshipEntityModel.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisode(x)).ToList();
    }


    public Guid ApprenticeshipKey { get; }
    public long ApprovalsApprenticeshipId { get; }
    public string Uln { get; }

    public long? FundingEmployerAccountId { get; }


    public List<ApprenticeshipEpisode> ApprenticeshipEpisodes { get; }

    public void CalculateEarnings(ISystemClock systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.CalculateEarnings();
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, decimal newAgreedPrice, DateTime effectiveFromDate)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        var existingEarnings = currentEpisode.EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        currentEpisode.UpdateAgreedPrice(systemClock, newAgreedPrice);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateStartDate(newStartDate, newEndDate, ageAtStartOfApprenticeship);
        currentEpisode.RecalculateEarnings(systemClock, apprenticeshipFunding => apprenticeshipFunding.RecalculateEarnings(newStartDate));

        AddEvent(new EarningsRecalculatedEvent(this));
    }
}

public static class ApprenticeshipExtensions
{
    public static ApprenticeshipEpisode GetCurrentEpisode(this Apprenticeship apprenticeship, ISystemClock systemClock)
    {
        var episode = apprenticeship.ApprenticeshipEpisodes.FirstOrDefault(x => x.ActualStartDate <= systemClock.UtcNow && x.PlannedEndDate >= systemClock.UtcNow);
        
        if(episode == null)
        {
            // if no episode is active for the current date, then there could be a episode for the apprenticeship that is yet to start
            episode = apprenticeship.ApprenticeshipEpisodes.Single(x => x.ActualStartDate >= systemClock.UtcNow);
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}