using Microsoft.Extensions.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{

    private ApprenticeshipEpisode(EpisodeModel model)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        if (_model.EarningsProfile != null)
        {
            _earningsProfile = EarningsProfile.Get(_model.EarningsProfile);
        }
    }

    private readonly EpisodeModel _model;
    private List<Price> _prices;
    private EarningsProfile _earningsProfile;

    public Guid ApprenticeshipEpisodeKey => _model.Key;
    public long UKPRN => _model.Ukprn;
    public long EmployerAccountId => _model.EmployerAccountId;
    public int AgeAtStartOfApprenticeship => _model.AgeAtStartOfApprenticeship;
    public string TrainingCode => _model.TrainingCode;
    public FundingType FundingType => _model.FundingType;
    public string LegalEntityName => _model.LegalEntityName;
    public long? FundingEmployerAccountId => _model.FundingEmployerAccountId;
    public EarningsProfile? EarningsProfile => _earningsProfile;
    public IReadOnlyCollection<Price> Prices => new ReadOnlyCollection<Price>(_prices);
    public bool IsNonLevyFullyFunded => _model.FundingType == FundingType.NonLevy && _model.AgeAtStartOfApprenticeship < 22;

    public string FundingLineType =>
        AgeAtStartOfApprenticeship < 19
            ? "16-18 Apprenticeship (Employer on App Service)"
            : "19+ Apprenticeship (Employer on App Service)";

    public static ApprenticeshipEpisode Get(EpisodeModel entity)
    {
        return new ApprenticeshipEpisode(entity);
    }

    public void CalculateEpisodeEarnings(Apprenticeship apprenticeship, ISystemClockService systemClock)
    {
        var earnings = OnProgramPayments.GenerateEarningsForEpisodePrices(Prices, out var onProgramTotal, out var completionPayment);
        var additionalPayments = IncentivePayments.GenerateIncentivePayments(
            AgeAtStartOfApprenticeship, 
            _prices.Min(p => p.StartDate), 
            _prices.Max(p => p.EndDate),
            apprenticeship.HasEHCP,
            apprenticeship.IsCareLeaver,
            apprenticeship.CareLeaverEmployerConsentGiven);
        UpdateEarningsProfile(earnings, additionalPayments, systemClock, onProgramTotal, completionPayment);
    }

    public void Update(Apprenticeships.Types.ApprenticeshipEpisode episodeUpdate)
    {
        UpdatePrices(episodeUpdate);

        _model.AgeAtStartOfApprenticeship = episodeUpdate.AgeAtStartOfApprenticeship;
        _model.EmployerAccountId = episodeUpdate.EmployerAccountId;
        _model.FundingEmployerAccountId = episodeUpdate.FundingEmployerAccountId;
        _model.FundingType = Enum.Parse<FundingType>(episodeUpdate.FundingType.ToString());
        _model.LegalEntityName = episodeUpdate.LegalEntityName;
        _model.TrainingCode = episodeUpdate.TrainingCode;
        _model.Ukprn = episodeUpdate.Ukprn;
    }

    public void RemoveEarningsAfter(DateTime lastDayOfLearning, ISystemClockService systemClock)
    {
        if (EarningsProfile != null)
        {
            var historyEntity = new EarningsProfileHistoryModel(EarningsProfile.GetModel(), systemClock!.UtcNow.Date);
            _model.EarningsProfileHistory.Add(historyEntity);
        }

        var earningsToKeep = GetEarningsToKeep(lastDayOfLearning);
        var additionalPaymentsToKeep = GetAdditionalPaymentsToKeep(lastDayOfLearning);

        _earningsProfile = new EarningsProfile(_model.EarningsProfile.OnProgramTotal, 
            earningsToKeep.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.EpisodePriceKey)).ToList(),
            additionalPaymentsToKeep.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList(),
            EarningsProfile.MathsAndEnglishCourses.Select(x => new MathsAndEnglish(x.StartDate, x.EndDate, x.Course, x.Amount, x.Instalments.ToList())).ToList(),
            _model.EarningsProfile.CompletionPayment, ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// </summary>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, ISystemClockService systemClock)
    {
        // verify that all additional payments are of the same type
        if (additionalPayments.Select(x => x.AdditionalPaymentType).Distinct().Count() > 1)
        {
            throw new InvalidOperationException("All additional payments must be of the same type.");
        }
        var additionalPaymentType = additionalPayments.First().AdditionalPaymentType;

        ArchiveEarningProfileToHistory(systemClock);

        // Retain only additional payments of a different type
        var existingAdditionalPayments = EarningsProfile!.AdditionalPayments.Where(x => x.AdditionalPaymentType != additionalPaymentType).ToList();

        existingAdditionalPayments.AddRange(additionalPayments);

        _earningsProfile = new EarningsProfile(
            EarningsProfile.OnProgramTotal,
            EarningsProfile.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.EpisodePriceKey)).ToList(),
            existingAdditionalPayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList(),
            EarningsProfile.MathsAndEnglishCourses.Select(x => new MathsAndEnglish(x.StartDate, x.EndDate, x.Course, x.Amount, x.Instalments.ToList())).ToList(),
            EarningsProfile.CompletionPayment,
            ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        ArchiveEarningProfileToHistory(systemClock);

        _earningsProfile = new EarningsProfile(
            EarningsProfile.OnProgramTotal,
            EarningsProfile.Instalments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.EpisodePriceKey)).ToList(),
            EarningsProfile.AdditionalPayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList(),
            mathsAndEnglishCourses.Select(x => new MathsAndEnglish(x.StartDate, x.EndDate, x.Course, x.Amount, x.Instalments.ToList())).ToList(),
            EarningsProfile.CompletionPayment,
            ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }

    private List<AdditionalPaymentModel> GetAdditionalPaymentsToKeep(DateTime lastDayOfLearning)
    {
        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();

        var additionalPayments = _model.EarningsProfile.AdditionalPayments
            .Where(x =>
                x.AcademicYear < academicYear //keep earnings from previous academic years
                || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
                || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Day ==
                DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month))
            .ToList(); //keep earnings in the last delivery period of learning if the learner is in learning on the census date

        return additionalPayments;
    }

    private List<InstalmentModel> GetEarningsToKeep(DateTime lastDayOfLearning)
    {
        List<InstalmentModel> result;

        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();

        var startDate = _model.Prices.Min(x => x.StartDate);
        var qualifyingDate = startDate.AddDays(Constants.QualifyingPeriod); //With shorter apprenticeships, this qualifying period will change
        if (lastDayOfLearning < qualifyingDate)
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear) //keep earnings from previous academic years
                .ToList();
        }
        else
        {
            result = _model.EarningsProfile.Instalments.Where(x =>
                    x.AcademicYear < academicYear //keep earnings from previous academic years
            || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
            || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Day == DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month)) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
                .ToList();
        }

        return result;
    }

    private void UpdatePrices(Apprenticeships.Types.ApprenticeshipEpisode episodeUpdate)
    {
        foreach (var existingPrice in _prices.ToList())
        {
            var updatedPrice = episodeUpdate.Prices.SingleOrDefault(x => x.Key == existingPrice.PriceKey);
            if (updatedPrice != null)
            {
                existingPrice.Update(updatedPrice.StartDate, updatedPrice.EndDate, updatedPrice.TotalPrice, updatedPrice.FundingBandMaximum);
            }
            else
            {
                _prices.Remove(existingPrice);
                _model.Prices.Remove(existingPrice.GetModel());
            }
        }

        var newPrices = episodeUpdate.Prices
            .Where(x => _prices.All(y => y.PriceKey != x.Key))
            .Select(x => new Price(x.Key, x.StartDate, x.EndDate, x.TotalPrice, x.FundingBandMaximum))
            .ToList();
        _model.Prices.AddRange(newPrices.Select(x => x.GetModel()));
        _prices.AddRange(newPrices);
    }

    private void UpdateEarningsProfile(IEnumerable<OnProgramPayment> onProgramPayments, IEnumerable<IncentivePayment> incentivePayments, ISystemClockService systemClock, decimal onProgramTotal, decimal completionPayment)
    {
        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var additionalPayments = incentivePayments.Select(x =>
            new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.IncentiveType)).ToList();

        List<MathsAndEnglish> mathsAndEnglishCourses = new List<MathsAndEnglish>();

        if (EarningsProfile != null)
        {
            ArchiveEarningProfileToHistory(systemClock);

            // Extract non calculated additional payments from the existing earnings profile
            var additionalPaymentsToKeep = EarningsProfile.PersistentAdditionalPayments();
            additionalPayments.AddRange(additionalPaymentsToKeep.Select(x =>
                new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList());

            // Extract maths and english payments from the existing earnings profile
            mathsAndEnglishCourses.AddRange(EarningsProfile.PersistentMathsAndEnglishCourses().ToList());
        }

        _earningsProfile = new EarningsProfile(onProgramTotal, instalments, additionalPayments, mathsAndEnglishCourses, completionPayment, ApprenticeshipEpisodeKey);
        _model.EarningsProfile = _earningsProfile.GetModel();
    }

    private void ArchiveEarningProfileToHistory(ISystemClockService systemClock)
    {
        var historyEntity = new EarningsProfileHistoryModel(EarningsProfile!.GetModel(), systemClock!.UtcNow.Date);
        _model.EarningsProfileHistory.Add(historyEntity);
    }
}