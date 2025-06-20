using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode : AggregateComponent
{

    private ApprenticeshipEpisode(EpisodeModel model, Action<AggregateComponent> addChildToRoot):base(addChildToRoot)
    {
        _model = model;

        _prices = _model.Prices.Select(Price.Get).ToList();
        if (_model.EarningsProfile != null)
        {
            _earningsProfile = this.GetEarningsProfileFromModel(_model.EarningsProfile);
        }
    }

    internal static ApprenticeshipEpisode Get(Apprenticeship apprenticeship, EpisodeModel entity)
    {
        var episode = new ApprenticeshipEpisode(entity, apprenticeship.AddChildToRoot);
        return episode;
    }

    private readonly EpisodeModel _model;
    private List<Price> _prices;
    private EarningsProfile? _earningsProfile;

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

    public void CalculateEpisodeEarnings(Apprenticeship apprenticeship, ISystemClockService systemClock)
    {
        var onProgramPayments = OnProgramPayments.GenerateEarningsForEpisodePrices(Prices, out var onProgramTotal, out var completionPayment);
        var instalments = onProgramPayments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();

        var incentivePayments = IncentivePayments.GenerateIncentivePayments(
            AgeAtStartOfApprenticeship, 
            _prices.Min(p => p.StartDate), 
            _prices.Max(p => p.EndDate),
            apprenticeship.HasEHCP,
            apprenticeship.IsCareLeaver,
            apprenticeship.CareLeaverEmployerConsentGiven);

        var additionalPayments = incentivePayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.IncentiveType)).ToList();

        if(_earningsProfile == null)
        {
            _earningsProfile = this.CreateEarningsProfile(onProgramTotal, instalments, additionalPayments, new List<MathsAndEnglish>(), completionPayment, ApprenticeshipEpisodeKey);
            _model.EarningsProfile = _earningsProfile.GetModel();
        }
        else
        {
            _earningsProfile.Update(systemClock, 
                instalments: instalments, 
                additionalPayments:additionalPayments,
                onProgramTotal:onProgramTotal,
                completionPayment:completionPayment);
        }

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
        var earningsToKeep = GetEarningsToKeep(lastDayOfLearning);
        var additionalPaymentsToKeep = GetAdditionalPaymentsToKeep(lastDayOfLearning);

        _earningsProfile.Update(
            systemClock,
            instalments: earningsToKeep.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.EpisodePriceKey)).ToList(),
            additionalPayments: additionalPaymentsToKeep.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList()
            );

    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// </summary>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, string additionalPaymentType, ISystemClockService systemClock)
    {
        // verify that all additional payments are of the same type
        if (additionalPayments.Select(x => x.AdditionalPaymentType).Distinct().Count() > 1)
        {
            throw new InvalidOperationException("All additional payments must be of the same type.");
        }

        // Retain only additional payments of a different type
        var existingAdditionalPayments = EarningsProfile!.AdditionalPayments.Where(x => x.AdditionalPaymentType != additionalPaymentType).ToList();

        existingAdditionalPayments.AddRange(additionalPayments);

        _earningsProfile.Update(
            systemClock,
            additionalPayments: existingAdditionalPayments.Select(x => new AdditionalPayment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.DueDate, x.AdditionalPaymentType)).ToList());
    }

    /// <summary>
    /// Updates earnings for Maths and English courses to an apprenticeship.
    /// Overwrites any existing Maths and English courses' earnings.
    /// </summary>
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> mathsAndEnglishCourses, ISystemClockService systemClock)
    {
        _earningsProfile.Update(systemClock,
            mathsAndEnglishCourses: mathsAndEnglishCourses.Select(x => new MathsAndEnglish(x.StartDate, x.EndDate, x.Course, x.Amount, x.Instalments.Select(i => new MathsAndEnglishInstalment(i.AcademicYear, i.DeliveryPeriod, i.Amount)).ToList())).ToList());
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
}