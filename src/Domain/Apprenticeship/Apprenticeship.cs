using Microsoft.Extensions.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System.Reflection;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    public Apprenticeship(ApprenticeshipEntityModel apprenticeshipEntityModel)
    {
        ApprenticeshipKey = apprenticeshipEntityModel.ApprenticeshipKey;
        ApprovalsApprenticeshipId = apprenticeshipEntityModel.ApprovalsApprenticeshipId;
        Uln = apprenticeshipEntityModel.Uln;
        LegalEntityName = apprenticeshipEntityModel.LegalEntityName;
        ActualStartDate = apprenticeshipEntityModel.ActualStartDate; //DO NOT APPROVE PR WITH THESE HERE
        PlannedEndDate = apprenticeshipEntityModel.PlannedEndDate; //DO NOT APPROVE PR WITH THESE HERE
        TrainingCode = apprenticeshipEntityModel.TrainingCode;
        FundingEmployerAccountId = apprenticeshipEntityModel.FundingEmployerAccountId;
        FundingType = apprenticeshipEntityModel.FundingType;
        FundingBandMaximum = apprenticeshipEntityModel.FundingBandMaximum;
        AgeAtStartOfApprenticeship = apprenticeshipEntityModel.AgeAtStartOfApprenticeship;

        ApprenticeshipEpisodes = apprenticeshipEntityModel.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisode(x)).ToList();
        EarningsProfile = apprenticeshipEntityModel.EarningsProfile != null ? new EarningsProfile(apprenticeshipEntityModel.EarningsProfile) : null;
    }


    public Guid ApprenticeshipKey { get; }
    public long ApprovalsApprenticeshipId { get; }
    public string Uln { get; }
    public string LegalEntityName { get; }


    public DateTime ActualStartDate { get; private set; } //DO NOT APPROVE PR WITH THESE HERE
    public DateTime PlannedEndDate { get; private set; } //DO NOT APPROVE PR WITH THESE HERE


    public string TrainingCode { get; }
    public long? FundingEmployerAccountId { get; }
    public FundingType FundingType { get; }
    public decimal FundingBandMaximum { get; }
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
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(currentEpisode.AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, decimal newAgreedPrice, DateTime effectiveFromDate)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateAgreedPrice(systemClock, newAgreedPrice);

        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(newAgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var existingEarnings = EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        var newEarnings = apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate);
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, newEarnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarnings(ISystemClock systemClock, DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);

        ActualStartDate = newStartDate;
        PlannedEndDate = newEndDate;
        AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;

        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(currentEpisode.AgreedPrice, newStartDate, newEndDate, FundingBandMaximum);
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
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }
}