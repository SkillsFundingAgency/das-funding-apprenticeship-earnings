using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public Guid ApprenticeshipEpisodeKey { get; }
    public long UKPRN { get; private set; }
    public long EmployerAccountId { get; private set; }
    public int AgeAtStartOfApprenticeship { get; private set; }
    public string TrainingCode { get; private set; }
    public FundingType FundingType { get; private set; }
    public string LegalEntityName { get; private set; }
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

    public void Update(Apprenticeships.Types.ApprenticeshipEpisode episodeUpdate)
    {
        Prices = episodeUpdate.Prices
            .Select(x => new Price(x.Key, x.StartDate, x.EndDate, x.TotalPrice, x.FundingBandMaximum))
            .ToList();
        AgeAtStartOfApprenticeship = episodeUpdate.AgeAtStartOfApprenticeship;
        EmployerAccountId = episodeUpdate.EmployerAccountId;
        FundingEmployerAccountId = episodeUpdate.FundingEmployerAccountId;
        FundingType = Enum.Parse<FundingType>(episodeUpdate.FundingType.ToString());
        LegalEntityName = episodeUpdate.LegalEntityName;
        TrainingCode = episodeUpdate.TrainingCode;
        UKPRN = episodeUpdate.Ukprn;
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