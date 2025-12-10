using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class UpdateOnProgrammeRequestBuilder
{
    private DateTime _priceStartDate = new DateTime(2020, 02, 01);
    private DateTime _priceEndDate = new DateTime(2022, 1, 1);
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceChangePriceKey = Guid.NewGuid();
    private decimal _newTrainingPrice = 17000;
    private decimal _newAssessmentPrice = 3000;
    private List<LearningEpisodePrice>? _existingPrices;

    public UpdateOnProgrammeRequestBuilder WithDataFromSetupModel(UpdateOnProgrammeModel model)
    {
        if (model.PriceStartDate.HasValue) _priceStartDate = model.PriceStartDate.Value;
        if (model.NewTrainingPrice.HasValue) _newTrainingPrice = model.NewTrainingPrice.Value;
        if (model.NewAssessmentPrice.HasValue) _newAssessmentPrice = model.NewAssessmentPrice.Value;
        if (model.PriceEndDate.HasValue) _priceEndDate = model.PriceEndDate.Value;
        return this;
    }

    public UpdateOnProgrammeRequestBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _episodeKey = apprenticeship.Episode.Key;
        _priceEndDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _existingPrices = apprenticeship.Episode.Prices;
        return this;
    }

    public UpdateOnProgrammeRequest Build(int fundingBandMaximum)
    {
        var prices = new List<LearningEpisodePrice>();

        if (_existingPrices != null && _existingPrices.Any())
        {
            _existingPrices.OrderBy(x => x.StartDate).Last().EndDate = _priceStartDate.AddDays(-1);
            prices.AddRange(_existingPrices);
        }

        prices.Add(new()
        {
            Key = _priceChangePriceKey,
            TrainingPrice = _newTrainingPrice,
            EndPointAssessmentPrice = _newAssessmentPrice,
            StartDate = _priceStartDate,
            EndDate = _priceEndDate,
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