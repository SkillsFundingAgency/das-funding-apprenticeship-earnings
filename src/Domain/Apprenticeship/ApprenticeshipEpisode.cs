using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public DateTime ActualStartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }
    public int AgeAtStartOfApprenticeship { get; private set; }
    public string TrainingCode { get; }
    public FundingType FundingType { get; }
    public decimal FundingBandMaximum { get; }
    public string LegalEntityName { get; }
    public EarningsProfile? EarningsProfile { get; private set; }
    public List<HistoryRecord<EarningsProfile>> EarningsProfileHistory { get; private set; }

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel model)
    {
        UKPRN = model.UKPRN;
        EmployerAccountId = model.EmployerAccountId;
        ActualStartDate = model.ActualStartDate;
        PlannedEndDate = model.PlannedEndDate;
        AgeAtStartOfApprenticeship = model.AgeAtStartOfApprenticeship;
        AgreedPrice = model.AgreedPrice;
        TrainingCode = model.TrainingCode;
        FundingType = model.FundingType;
        FundingBandMaximum = model.FundingBandMaximum;
        LegalEntityName = model.LegalEntityName;
        EarningsProfile = model.EarningsProfile != null ? new EarningsProfile(model.EarningsProfile) : null;

        if(model.EarningsProfileHistory != null)
        {
            EarningsProfileHistory = model.EarningsProfileHistory.Select(x => new HistoryRecord<EarningsProfile> { SupersededDate = x.SupersededDate, Record = new EarningsProfile(x.Record)}).ToList();
        }
        else
        {
            EarningsProfileHistory = new List<HistoryRecord<EarningsProfile>>();
        }

    }

    public void CalculateEarnings()
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        UpdateEarningsProfile(apprenticeshipFunding, earnings, null);
    }

    public void RecalculateEarnings(ISystemClock systemClock, Func<ApprenticeshipFunding.ApprenticeshipFunding, List<Earning>> recalculate)
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(AgreedPrice, ActualStartDate, PlannedEndDate, FundingBandMaximum);
        var newEarnings = recalculate(apprenticeshipFunding);
        UpdateEarningsProfile(apprenticeshipFunding, newEarnings, systemClock);
    }

    public void UpdateAgreedPrice(ISystemClock systemClock, decimal newAgreedPrice)
    {
        AgreedPrice = newAgreedPrice;
        // PlannedEndDate = systemClock.UtcNow.DateTime; // TO BE COMPLETED IN SUBTASK FLP-800
    }

    public void UpdateStartDate(DateTime startDate, DateTime endDate, int ageAtStartOfApprenticeship) 
    { 
        ActualStartDate = startDate;
        PlannedEndDate = endDate;
        AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;
        // THIS HANDLING MAY NEED TO BE REFINED IN SUBTASK FLP-801
    }

    private void UpdateEarningsProfile(ApprenticeshipFunding.ApprenticeshipFunding apprenticeshipFunding, List<Earning> earnings, ISystemClock? systemClock)
    {
        if (EarningsProfile != null) 
        {
            EarningsProfileHistory.Add(new HistoryRecord<EarningsProfile> { Record = EarningsProfile, SupersededDate = systemClock!.UtcNow.Date });
        }
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
    }
}