using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class PriceChangeSavePricesRequestBuilder
{
    private DateTime _effectiveFromDate = new DateTime(2020, 02, 01);
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceChangePriceKey = Guid.NewGuid();
    private decimal _newTrainingPrice = 17000;
    private decimal _newAssessmentPrice = 3000;
    private DateTime _endDate = new DateTime(2022, 1, 1);
    private List<LearningEpisodePrice>? _existingPrices;

    public PriceChangeSavePricesRequestBuilder WithDataFromSetupModel(UpdateOnProgrammeModel model)
    {
        if (model.PriceStartDate.HasValue) _effectiveFromDate = model.PriceStartDate.Value;
        if (model.PriceEndDate.HasValue) _endDate = model.PriceEndDate.Value;
        if (model.NewTrainingPrice.HasValue) _newTrainingPrice = model.NewTrainingPrice.Value;
        if (model.NewAssessmentPrice.HasValue) _newAssessmentPrice = model.NewAssessmentPrice.Value;
        
        return this;
    }

    public PriceChangeSavePricesRequestBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _existingPrices = apprenticeship.Episode.Prices;
        return this;
    }

    public UpdateOnProgrammeRequest Build(int fundingBandMaximum)
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

        return new UpdateOnProgrammeRequest()
        {
            ApprenticeshipEpisodeKey = _episodeKey,
            FundingBandMaximum = fundingBandMaximum,
            Prices = prices,
            IncludesFundingBandMaximumUpdate = true
        };
    }
}