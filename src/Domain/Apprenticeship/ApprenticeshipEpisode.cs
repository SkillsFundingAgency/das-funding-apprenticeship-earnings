using SFA.DAS.Apprenticeships.Types;
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
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(Prices![0].AgreedPrice, Prices[0].ActualStartDate, Prices[0].PlannedEndDate, Prices[0].FundingBandMaximum);
        var earnings = apprenticeshipFunding.GenerateEarnings();
        UpdateEarningsProfile(apprenticeshipFunding, earnings, null);
    }

    public void RecalculateEarnings(ISystemClockService systemClock, Func<ApprenticeshipFunding.ApprenticeshipFunding, List<Earning>> recalculate)
    {
        var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(
            this.GetCurrentPrice(systemClock).AgreedPrice,
            Prices!.OrderBy(x => x.ActualStartDate).First().ActualStartDate,
            Prices!.OrderBy(x => x.PlannedEndDate).Last().PlannedEndDate,
            this.GetCurrentPrice(systemClock).FundingBandMaximum);
        var newEarnings = recalculate(apprenticeshipFunding);
        UpdateEarningsProfile(apprenticeshipFunding, newEarnings, systemClock);
    }

    public void UpdateAgreedPrice(ISystemClockService systemClock, decimal newAgreedPrice, List<Guid> deletedPriceKeys, Guid newPriceKey)
    {
        // update existing price with matching key if it exists
        var existingPrice = Prices?.SingleOrDefault(x => x.PriceKey == newPriceKey);
        if (existingPrice != null)
        {
            existingPrice.UpdatePrice(newAgreedPrice);
            Prices!.RemoveAll(x => deletedPriceKeys.Exists(key => key == x.PriceKey));
            return;
        }

        // build new price
        var newPriceStartDate = systemClock.UtcNow.Date;
        var newPrice = new Price(
            newPriceKey,
            newPriceStartDate,
            Prices!.OrderByDescending(x => x.PlannedEndDate).First().PlannedEndDate,
            newAgreedPrice,
            Prices!.OrderBy(x => x.ActualStartDate).First().FundingBandMaximum);

        // remove all deleted prices
        Prices!.RemoveAll(x => deletedPriceKeys.Exists(key => key == x.PriceKey));

        // close off remaining active price
        Prices.SingleOrDefault()!.CloseOff(newPriceStartDate.AddDays(-1));
        
        // add new price
        Prices.Add(newPrice);
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