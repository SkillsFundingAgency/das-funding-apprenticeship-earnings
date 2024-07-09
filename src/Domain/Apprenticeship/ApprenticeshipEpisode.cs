using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public Guid ApprenticeshipEpisodeKey { get; }
    public long UKPRN { get; }
    public long EmployerAccountId { get; }
    public int AgeAtStartOfApprenticeship { get; private set; }
    public string TrainingCode { get; }
    public FundingType FundingType { get; }
    public string LegalEntityName { get; }
    public long? FundingEmployerAccountId { get; set; }
    public EarningsProfile? EarningsProfile { get; private set; }
    public List<Price>? Prices { get; private set; }
    public List<HistoryRecord<EarningsProfile>> EarningsProfileHistory { get; private set; }

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel model)
    {
        ApprenticeshipEpisodeKey = model.ApprenticeshipEpisodeKey;
        UKPRN = model.UKPRN;
        EmployerAccountId = model.EmployerAccountId;
        TrainingCode = model.TrainingCode;
        FundingType = model.FundingType;
        LegalEntityName = model.LegalEntityName;
        EarningsProfile = model.EarningsProfile != null ? new EarningsProfile(model.EarningsProfile) : null;
        FundingEmployerAccountId = model.FundingEmployerAccountId;
        AgeAtStartOfApprenticeship = model.AgeAtStartOfApprenticeship;

        if(model.EarningsProfileHistory != null)
        {
            EarningsProfileHistory = model.EarningsProfileHistory.Select(x => new HistoryRecord<EarningsProfile> { SupersededDate = x.SupersededDate, Record = new EarningsProfile(x.Record)}).ToList();
        }
        else
        {
            EarningsProfileHistory = new List<HistoryRecord<EarningsProfile>>();
        }

        Prices = model.Prices != null ? model.Prices.Select(x => new Price(x)).ToList() : new List<Price>();
    }

    public void CalculateEarnings()
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(Prices.First().AgreedPrice, Prices.First().ActualStartDate, Prices.First().PlannedEndDate, Prices.First().FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        UpdateEarningsProfile(apprenticeshipFunding, earnings, null);
    }

    public void RecalculateEarnings(ISystemClockService systemClock, Func<ApprenticeshipFunding.ApprenticeshipFunding, List<Earning>> recalculate)
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(Prices.First().AgreedPrice, Prices.First().ActualStartDate, Prices.First().PlannedEndDate, Prices.First().FundingBandMaximum);
        var newEarnings = recalculate(apprenticeshipFunding);
        UpdateEarningsProfile(apprenticeshipFunding, newEarnings, systemClock);
    }

    public void UpdateAgreedPrice(ISystemClockService systemClock, decimal newAgreedPrice, List<Guid> deletedPriceKeys, Guid newPriceKey)
    {
        var newPrice = new Price(
            newPriceKey,
            Prices.OrderBy(x => x.ActualStartDate).First().ActualStartDate,
            Prices.OrderByDescending(x => x.PlannedEndDate).First().PlannedEndDate,
            newAgreedPrice,
            Prices.OrderBy(x => x.ActualStartDate).First().FundingBandMaximum);

        Prices.RemoveAll(x => deletedPriceKeys.Exists(key => key == x.PriceKey));
        
        Prices.Add(newPrice);

        // todo update correct Price based on logic in design AgreedPrice = newAgreedPrice;
        // PlannedEndDate = systemClock.UtcNow.DateTime; // TO BE COMPLETED IN SUBTASK FLP-800W
    }

    public void UpdateStartDate(DateTime startDate, DateTime endDate, int ageAtStartOfApprenticeship, List<Guid> deletedPriceKeys, Guid changingPriceKey)
    {
        Prices.RemoveAll(x => deletedPriceKeys.Exists(key => key == x.PriceKey));
        Prices.Find(x => x.PriceKey == changingPriceKey).UpdateDates(startDate, endDate);
        
        AgeAtStartOfApprenticeship = ageAtStartOfApprenticeship;
    }

    private void UpdateEarningsProfile(ApprenticeshipFunding.ApprenticeshipFunding apprenticeshipFunding, List<Earning> earnings, ISystemClockService? systemClock)
    {
        if (EarningsProfile != null) 
        {
            EarningsProfileHistory.Add(new HistoryRecord<EarningsProfile> { Record = EarningsProfile, SupersededDate = systemClock!.UtcNow.Date });
        }
        EarningsProfile = new EarningsProfile(apprenticeshipFunding.OnProgramTotal, earnings.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList(), apprenticeshipFunding.CompletionPayment, Guid.NewGuid());
    }
}