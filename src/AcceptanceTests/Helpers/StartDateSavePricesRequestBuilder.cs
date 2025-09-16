using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class StartDateSavePricesRequestBuilder
{
    private Guid _learningKey = Guid.NewGuid();
    private long _apprenticeshipId = 123;
    private DateTime _startDate = new DateTime(2024, 8, 1);
    private DateTime _approvedDate = DateTime.UtcNow;
    private string _providerApprovedBy = "";
    private string _employerApprovedBy = "";
    private string _initiator = "";
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceKey = Guid.NewGuid();
    private DateTime _endDate = new DateTime(2025, 8, 1);
    private int _fundingBandMaximum = 18000;
    private long _employerAccountId = 456;
    private int _ageAtStartOfLearning = 19;

    public StartDateSavePricesRequestBuilder WithLearningKey(Guid key)
    {
        _learningKey = key;
        return this;
    }

    public StartDateSavePricesRequestBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public StartDateSavePricesRequestBuilder WithFundingBandMaximum(int max)
    {
        _fundingBandMaximum = max;
        return this;
    }

    public StartDateSavePricesRequestBuilder WithAgeAtStart(int age)
    {
        _ageAtStartOfLearning = age;
        return this;
    }

    public StartDateSavePricesRequestBuilder WithDuration(int months)
    {
        _endDate = _startDate.AddMonths(months);
        return this;
    }

    public StartDateSavePricesRequestBuilder WithAdjustedStartDateBy(int months)
    {
        _startDate = _startDate.AddMonths(months);
        return this;
    }

    public StartDateSavePricesRequestBuilder WithAdjustedEndDateBy(int months)
    {
        _endDate = _endDate.AddMonths(months);
        return this;
    }

    public StartDateSavePricesRequestBuilder WithDataFromSetupModel(StartDateChangeModel model)
    {
        if (model.NewStartDate.HasValue) _startDate = model.NewStartDate.Value;
        if (model.ApprovedDate.HasValue) _approvedDate = model.ApprovedDate.Value;
        return this;
    }

    public StartDateSavePricesRequestBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _learningKey = apprenticeship.LearningKey;
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _fundingBandMaximum = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().FundingBandMaximum;
        _ageAtStartOfLearning = apprenticeship.Episode.AgeAtStartOfLearning;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        return this;
    }

    public SavePricesRequest Build()
    {
        var prices = new List<LearningEpisodePrice>
        {
            new()
            {
                Key = _priceKey,
                StartDate = _startDate,
                EndDate = _endDate,
                TotalPrice = 15000,
                FundingBandMaximum = _fundingBandMaximum
            }
        };

        return new SavePricesRequest()
        {
            ApprenticeshipEpisodeKey = _episodeKey,
            Prices = prices
        };
    }
}