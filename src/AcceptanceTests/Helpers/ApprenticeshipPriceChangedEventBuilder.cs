using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class LearningPriceChangedEventBuilder
{
    private Guid _learningKey = Guid.NewGuid();
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
    private int _ageAtStartOfLearning = 20;
    private List<LearningEpisodePrice>? _existingPrices;

    public LearningPriceChangedEventBuilder WithLearningKey(Guid key)
    {
        _learningKey = key;
        return this;
    }

    public LearningPriceChangedEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public LearningPriceChangedEventBuilder WithEffectiveFromDate(DateTime date)
    {
        _effectiveFromDate = date;
        return this;
    }

    public LearningPriceChangedEventBuilder WithApprovedBy(ApprovedBy approvedBy)
    {
        _approvedBy = approvedBy;
        return this;
    }

    public LearningPriceChangedEventBuilder WithApprovedDate(DateTime date)
    {
        _approvedDate = date;
        return this;
    }

    public LearningPriceChangedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public LearningPriceChangedEventBuilder WithExistingPriceKey(Guid existingPriceKey)
    {
        _existingPriceKey = existingPriceKey;
        return this;
    }

    public LearningPriceChangedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public LearningPriceChangedEventBuilder WithFundingBandMaximum(int fundingBandMaximum)
    {
        _fundingBandMaximum = fundingBandMaximum;
        return this;
    }

    public LearningPriceChangedEventBuilder WithPriceChangePriceKey(Guid priceChangePriceKey)
    {
        _priceChangePriceKey = priceChangePriceKey;
        return this;
    }

    public LearningPriceChangedEventBuilder WithNewTrainingPrice(decimal newTrainingPrice)
    {
        _newTrainingPrice = newTrainingPrice;
        return this;
    }

    public LearningPriceChangedEventBuilder WithNewAssessmentPrice(decimal newAssessmentPrice)
    {
        _newAssessmentPrice = newAssessmentPrice;
        return this;
    }

    public LearningPriceChangedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public LearningPriceChangedEventBuilder WithEmployerAccountId(long employerAccountId)
    {
        _employerAccountId = employerAccountId;
        return this;
    }

    public LearningPriceChangedEventBuilder WithAgeAtStartOfLearning(int AgeAtStartOfLearning)
    {
        _ageAtStartOfLearning = AgeAtStartOfLearning;
        return this;
    }

    public LearningPriceChangedEventBuilder WithDataFromSetupModel(PriceChangeModel model)
    {
        if (model.EffectiveFromDate.HasValue) _effectiveFromDate = model.EffectiveFromDate.Value;
        if (model.ChangeRequestDate.HasValue) _approvedDate = model.ChangeRequestDate.Value;
        if (model.NewTrainingPrice.HasValue) _newTrainingPrice = model.NewTrainingPrice.Value;
        if (model.NewAssessmentPrice.HasValue) _newAssessmentPrice = model.NewAssessmentPrice.Value;
        return this;
    }

    public LearningPriceChangedEventBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _learningKey = apprenticeship.LearningKey;
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _fundingBandMaximum = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().FundingBandMaximum;
        _ageAtStartOfLearning = apprenticeship.Episode.AgeAtStartOfLearning;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        _existingPrices = apprenticeship.Episode.Prices;
        return this;
    }

    public LearningPriceChangedEvent Build()
    {
        var prices = new List<LearningEpisodePrice>();

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

        return new LearningPriceChangedEvent()
        {
            LearningKey = _learningKey,
            ApprovalsApprenticeshipId = _apprenticeshipId,
            EffectiveFromDate = _effectiveFromDate,
            ApprovedBy = _approvedBy,
            ApprovedDate = _approvedDate,
            Episode = new LearningEpisode
            {
                Key = _episodeKey,
                Prices = prices,
                EmployerAccountId = _employerAccountId,
                Ukprn = 123,
                LegalEntityName = "Smiths",
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                AgeAtStartOfLearning = _ageAtStartOfLearning,
                FundingPlatform = Learning.Enums.FundingPlatform.DAS
            }
        };
    }
}