using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class PriceChangeSavePricesRequestBuilder
{
    private DateTime _effectiveFromDate = new DateTime(2020, 02, 01);
    private Guid _episodeKey = Guid.NewGuid();
    private int _fundingBandMaximum = EventBuilderSharedDefaults.FundingBandMaximum;
    private Guid _priceChangePriceKey = Guid.NewGuid();
    private decimal _newTrainingPrice = 17000;
    private decimal _newAssessmentPrice = 3000;
    private DateTime _endDate = new DateTime(2022, 1, 1);
    private List<LearningEpisodePrice>? _existingPrices;

    public PriceChangeSavePricesRequestBuilder WithDataFromSetupModel(PriceChangeModel model)
    {
        if (model.EffectiveFromDate.HasValue) _effectiveFromDate = model.EffectiveFromDate.Value;
        if (model.NewTrainingPrice.HasValue) _newTrainingPrice = model.NewTrainingPrice.Value;
        if (model.NewAssessmentPrice.HasValue) _newAssessmentPrice = model.NewAssessmentPrice.Value;
        return this;
    }

    public PriceChangeSavePricesRequestBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _fundingBandMaximum = apprenticeship.Episode.FundingBandMaximum;
        _existingPrices = apprenticeship.Episode.Prices;
        return this;
    }

    public SavePricesRequest Build()
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
            TotalPrice = _newTrainingPrice + _newAssessmentPrice
        });

        return new SavePricesRequest()
        {
            ApprenticeshipEpisodeKey = _episodeKey,
            Prices = prices
        };
    }
}