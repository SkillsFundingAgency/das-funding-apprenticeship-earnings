using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class ApprenticeshipPriceChangedEventBuilder
{
    private Guid _apprenticeshipKey = Guid.NewGuid();
    private long _apprenticeshipId = EventBuilderSharedDefaults.ApprovalsApprenticeshipId;
    private DateTime _effectiveFromDate = new DateTime(2020, 02, 01);
    private ApprovedBy _approvedBy = ApprovedBy.Employer;
    private DateTime _approvedDate = new DateTime(2020, 1, 1);
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _existingPriceKey = Guid.NewGuid();
    private DateTime _startDate = new DateTime(2019, 9, 1);
    private int _fundingBandMaximum = EventBuilderSharedDefaults.FundingBandMaximum;
    private Guid _priceChangePriceKey = Guid.NewGuid();
    private decimal _newTrainingPrice = 17000;
    private decimal _newAssessmentPrice = 3000;
    private DateTime _endDate = new DateTime(2022, 1, 1);
    private long _employerAccountId = EventBuilderSharedDefaults.EmployerAccountId;
    private int _ageAtStartOfApprenticeship = 20;
    private List<ApprenticeshipEpisodePrice>? _existingPrices;

    public ApprenticeshipPriceChangedEventBuilder WithApprenticeshipKey(Guid key)
    {
        _apprenticeshipKey = key;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithEffectiveFromDate(DateTime date)
    {
        _effectiveFromDate = date;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithApprovedBy(ApprovedBy approvedBy)
    {
        _approvedBy = approvedBy;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithApprovedDate(DateTime date)
    {
        _approvedDate = date;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithExistingPriceKey(Guid existingPriceKey)
    {
        _existingPriceKey = existingPriceKey;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithFundingBandMaximum(int fundingBandMaximum)
    {
        _fundingBandMaximum = fundingBandMaximum;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithPriceChangePriceKey(Guid priceChangePriceKey)
    {
        _priceChangePriceKey = priceChangePriceKey;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithNewTrainingPrice(decimal newTrainingPrice)
    {
        _newTrainingPrice = newTrainingPrice;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithNewAssessmentPrice(decimal newAssessmentPrice)
    {
        _newAssessmentPrice = newAssessmentPrice;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithEmployerAccountId(long employerAccountId)
    {
        _employerAccountId = employerAccountId;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithAgeAtStartOfApprenticeship(int ageAtStartOfApprenticeship)
    {
        _ageAtStartOfApprenticeship = ageAtStartOfApprenticeship;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithDataFromSetupModel(PriceChangeModel model)
    {
        if (model.EffectiveFromDate.HasValue) _effectiveFromDate = model.EffectiveFromDate.Value;
        if (model.ChangeRequestDate.HasValue) _approvedDate = model.ChangeRequestDate.Value;
        if (model.NewTrainingPrice.HasValue) _newTrainingPrice = model.NewTrainingPrice.Value;
        if (model.NewAssessmentPrice.HasValue) _newAssessmentPrice = model.NewAssessmentPrice.Value;
        return this;
    }

    public ApprenticeshipPriceChangedEventBuilder WithExistingApprenticeshipData(ApprenticeshipCreatedEvent apprenticeship)
    {
        _apprenticeshipKey = apprenticeship.ApprenticeshipKey;
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _fundingBandMaximum = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().FundingBandMaximum;
        _ageAtStartOfApprenticeship = apprenticeship.Episode.AgeAtStartOfApprenticeship;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        _existingPrices = apprenticeship.Episode.Prices;
        return this;
    }

    public ApprenticeshipPriceChangedEvent Build()
    {
        var prices = new List<ApprenticeshipEpisodePrice>();

        if (_existingPrices != null && _existingPrices.Any())
        {
            _existingPrices.OrderBy(x => x.StartDate).Last().EndDate = _effectiveFromDate.AddDays(-1);
            prices.AddRange(_existingPrices);
        }

        prices.Add(new()
        {
            Key = _priceChangePriceKey,
            TrainingPrice = _newTrainingPrice,
            EndPointAssessmentPrice = _newAssessmentPrice,
            StartDate = _effectiveFromDate,
            EndDate = _endDate,
            FundingBandMaximum = _fundingBandMaximum,
            TotalPrice = _newTrainingPrice + _newAssessmentPrice
        });

        return new ApprenticeshipPriceChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipKey,
            ApprenticeshipId = _apprenticeshipId,
            EffectiveFromDate = _effectiveFromDate,
            ApprovedBy = _approvedBy,
            ApprovedDate = _approvedDate,
            Episode = new ApprenticeshipEpisode
            {
                Key = _episodeKey,
                Prices = prices,
                EmployerAccountId = _employerAccountId,
                Ukprn = 123,
                LegalEntityName = "Smiths",
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS
            }
        };
    }
}