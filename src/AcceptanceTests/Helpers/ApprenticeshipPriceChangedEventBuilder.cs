using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class ApprenticeshipPriceChangedEventBuilder
{
    private Guid _apprenticeshipKey = Guid.NewGuid();
    private long _apprenticeshipId = ApprenticeshipCreatedEventDefaults.ApprovalsApprenticeshipId;
    private DateTime _effectiveFromDate = ApprenticeshipPriceChangedEventDefaults.EffectiveFromDate;
    private ApprovedBy _approvedBy = ApprenticeshipPriceChangedEventDefaults.ApprovedBy;
    private DateTime _approvedDate = ApprenticeshipPriceChangedEventDefaults.ApprovedDate;
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _existingPriceKey = Guid.NewGuid();
    private DateTime _startDate = ApprenticeshipPriceChangedEventDefaults.StartDate;
    private int _fundingBandMaximum = ApprenticeshipCreatedEventDefaults.FundingBandMaximum;
    private Guid _priceChangePriceKey = Guid.NewGuid();
    private decimal _newTrainingPrice = 17000; //todo do the defaults actually add any value?
    private decimal _newAssessmentPrice = 3000;
    private DateTime _endDate = new DateTime(2022, 1, 1);
    private long _employerAccountId = 112;
    private int _ageAtStartOfApprenticeship = 20;

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

    public ApprenticeshipPriceChangedEvent Build()
    {
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
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        Key = _existingPriceKey,
                        StartDate = _startDate,
                        EndDate = _effectiveFromDate.AddDays(-1),
                        FundingBandMaximum = _fundingBandMaximum,
                        TotalPrice = 15000
                    },
                    new()
                    {
                        Key = _priceChangePriceKey,
                        TrainingPrice = _newTrainingPrice,
                        EndPointAssessmentPrice = _newAssessmentPrice,
                        StartDate = _effectiveFromDate,
                        EndDate = _endDate,
                        FundingBandMaximum = _fundingBandMaximum,
                        TotalPrice = _newTrainingPrice + _newAssessmentPrice
                    }
                },
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