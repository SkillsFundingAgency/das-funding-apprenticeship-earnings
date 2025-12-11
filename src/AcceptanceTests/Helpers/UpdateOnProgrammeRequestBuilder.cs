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
    private TrackedValue<DateTime> _priceStartDate = new TrackedValue<DateTime>(new DateTime(2020, 02, 01));
    private DateTime _priceEndDate = new DateTime(2022, 1, 1);
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceChangePriceKey = Guid.NewGuid();

    private DateTime _dateOfBirth = new DateTime(2000, 1, 1);

    private DateTime? _pauseDate = null;

    private TrackedValue<decimal> _newTrainingPrice = new TrackedValue<decimal>(17000);
    private TrackedValue<decimal> _newAssessmentPrice = new TrackedValue<decimal>(3000);
    private bool _hasPriceChanged => _newTrainingPrice.HasChanged || _newAssessmentPrice.HasChanged;
    private List<LearningEpisodePrice>? _existingPrices;
    private List<BreakInLearningItem> _breaksInLearning = new List<BreakInLearningItem>();
    private DateTime? _completionDate = null;


    public UpdateOnProgrammeRequestBuilder WithDataFromSetupModel(UpdateOnProgrammeModel model)
    {
        _priceStartDate.SetFromTrackedValue(model.PriceStartDate);
        _newTrainingPrice.SetFromTrackedValue(model.NewTrainingPrice);
        _newAssessmentPrice.SetFromTrackedValue(model.NewAssessmentPrice);
        if (model.PriceEndDate.HasChanged) _priceEndDate = model.PriceEndDate.Value;
        if (model.DateOfBirth.HasChanged) _dateOfBirth = model.DateOfBirth.Value.Value;
        if (model.PauseDate.HasChanged) _pauseDate = model.PauseDate.Value;
        if (model.BreaksInLearning.HasChanged) _breaksInLearning = model.BreaksInLearning.Value;
        if (model.CompletionDate.HasChanged) _completionDate = model.CompletionDate.Value;
        return this;
    }

    public UpdateOnProgrammeRequestBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _episodeKey = apprenticeship.Episode.Key;

        _priceStartDate.ResetValue(apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).First().StartDate);

        var lastEpisodePrice = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last();
        _priceEndDate = lastEpisodePrice.EndDate;
        _newTrainingPrice.ResetValue(lastEpisodePrice.TrainingPrice.Value);
        _newAssessmentPrice.ResetValue(lastEpisodePrice.EndPointAssessmentPrice.Value);

        _existingPrices = apprenticeship.Episode.Prices;
        _dateOfBirth = apprenticeship.DateOfBirth;

        return this;
    }

    public UpdateOnProgrammeRequest Build(int fundingBandMaximum)
    {
        var prices = new List<LearningEpisodePrice>();

        if (_existingPrices != null && _existingPrices.Any() && _hasPriceChanged)
        {
            _existingPrices.OrderBy(x => x.StartDate).Last().EndDate = _priceStartDate.Value.AddDays(-1);
            prices.AddRange(_existingPrices);
        }

        prices.Add(new()
        {
            Key = _priceChangePriceKey,
            TrainingPrice = _newTrainingPrice.Value,
            EndPointAssessmentPrice = _newAssessmentPrice.Value,
            StartDate = _priceStartDate.Value,
            EndDate = _priceEndDate,
            TotalPrice = _newTrainingPrice.Value + _newAssessmentPrice.Value
        });

        var requiresFundingBandMaximumUpdate = _hasPriceChanged || _priceStartDate.HasChanged;

        return new UpdateOnProgrammeRequest()
        {
            ApprenticeshipEpisodeKey = _episodeKey,
            DateOfBirth = _dateOfBirth,
            PauseDate = _pauseDate,
            CompletionDate = _completionDate,
            FundingBandMaximum = requiresFundingBandMaximumUpdate ? (int?)fundingBandMaximum : null,
            Prices = prices,
            IncludesFundingBandMaximumUpdate = requiresFundingBandMaximumUpdate,
            BreaksInLearning = _breaksInLearning
        };
    }
}

