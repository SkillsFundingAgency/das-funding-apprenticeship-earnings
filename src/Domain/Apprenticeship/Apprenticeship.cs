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
        LegalEntityName = apprenticeshipEntityModel.LegalEntityName;
        FundingEmployerAccountId = apprenticeshipEntityModel.FundingEmployerAccountId;
        AgeAtStartOfApprenticeship = apprenticeshipEntityModel.AgeAtStartOfApprenticeship;

        ApprenticeshipEpisodes = apprenticeshipEntityModel.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisode(x)).ToList();
        EarningsProfile = apprenticeshipEntityModel.EarningsProfile != null ? new EarningsProfile(apprenticeshipEntityModel.EarningsProfile) : null;
    }


    public Guid ApprenticeshipKey { get; }
    public long ApprovalsApprenticeshipId { get; }
    public string Uln { get; }
    public string LegalEntityName { get; }
    public long? FundingEmployerAccountId { get; }


    public List<ApprenticeshipEpisode> ApprenticeshipEpisodes { get; }
    public int AgeAtStartOfApprenticeship { get; private set; }
    public EarningsProfile? EarningsProfile { get; private set; }

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public void CalculateEarnings(ISystemClock systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(currentEpisode.AgreedPrice, currentEpisode.ActualStartDate, currentEpisode.PlannedEndDate, currentEpisode.FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, decimal newAgreedPrice, DateTime effectiveFromDate)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateAgreedPrice(systemClock, newAgreedPrice);

        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(newAgreedPrice, currentEpisode.ActualStartDate, currentEpisode.PlannedEndDate, currentEpisode.FundingBandMaximum);
        var existingEarnings = EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        var newEarnings = apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate);
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, newEarnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateStartDate(newStartDate,newEndDate);

        AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;

        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(currentEpisode.AgreedPrice, newStartDate, newEndDate, currentEpisode.FundingBandMaximum);
        var newEarnings = apprenticeshipFunding.RecalculateEarnings(newStartDate);
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, newEarnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
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