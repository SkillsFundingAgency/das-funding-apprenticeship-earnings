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
        EmployerAccountId = apprenticeshipEntityModel.EmployerAccountId;
        LegalEntityName = apprenticeshipEntityModel.LegalEntityName;
        ActualStartDate = apprenticeshipEntityModel.ActualStartDate;
        PlannedEndDate = apprenticeshipEntityModel.PlannedEndDate;
        AgreedPrice = apprenticeshipEntityModel.AgreedPrice;
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
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public string LegalEntityName { get; }
    public DateTime ActualStartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }
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

    public void CalculateEarnings()
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsCalculatedEvent(this));
    }

    public void RecalculateEarnings(decimal newAgreedPrice, DateTime effectiveFromDate)
    {
        AgreedPrice = newAgreedPrice;
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var existingEarnings = EarningsProfile.Instalments.Select(x => new Earning { AcademicYear = x.AcademicYear, Amount = x.Amount, DeliveryPeriod = x.DeliveryPeriod }).ToList();
        var newEarnings = apprenticeshipFunding.RecalculateEarnings(existingEarnings, effectiveFromDate);
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, newEarnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsRecalculatedEvent(this));
    }

    public void RecalculateEarnings(DateTime newStartDate, DateTime newEndDate, int ageAtStartOfApprenticeship)
    {
        ActualStartDate = newStartDate;
        PlannedEndDate = newEndDate;
        AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, newStartDate, newEndDate, FundingBandMaximum);
        var newEarnings = apprenticeshipFunding.RecalculateEarnings(newStartDate);
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, newEarnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
        AddEvent(new EarningsRecalculatedEvent(this));
    }
}