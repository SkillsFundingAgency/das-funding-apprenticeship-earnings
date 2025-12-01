using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class LearningCreatedEventBuilder
{
    private Guid _learningKey = Guid.NewGuid();
    private string _uln = new Random().Next().ToString();
    private long _approvalsApprenticeshipId = EventBuilderSharedDefaults.ApprovalsApprenticeshipId;
    private List<LearningEpisodePrice> _prices = new();
    private DateTime _startDate = new DateTime(2019, 01, 01);
    private DateTime _endDate = new DateTime(2021, 1, 1);
    private int _ageAtStart = 21;
    private Learning.Enums.FundingPlatform _fundingPlatform = Learning.Enums.FundingPlatform.DAS;
    private decimal _totalPrice = 15000m;
    private int _fundingBandMaximum = EventBuilderSharedDefaults.FundingBandMaximum;
    private long _employerAccountId = EventBuilderSharedDefaults.EmployerAccountId;
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceKey = Guid.NewGuid();
    private TestSystemClock _systemClock = new TestSystemClock();

    public LearningCreatedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public LearningCreatedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public LearningCreatedEventBuilder WithAgeAtStart(int age)
    {
        _ageAtStart = age;
        return this;
    }

    public LearningCreatedEventBuilder WithFundingPlatform(Learning.Enums.FundingPlatform platform)
    {
        _fundingPlatform = platform;
        return this;
    }

    public LearningCreatedEventBuilder WithPrices(List<LearningEpisodePrice> prices)
    {
        _prices = prices;
        return this;
    }

    public LearningCreatedEventBuilder WithTotalPrice(decimal totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }
    public LearningCreatedEventBuilder WithFundingBandMaximum(int fundingBandMaximum)
    {
        _fundingBandMaximum = fundingBandMaximum;
        return this;
    }

    public LearningCreatedEventBuilder WithDataFromSetupModel(ApprenticeshipCreatedSetupModel model)
    {
        if (model.Age.HasValue) _ageAtStart = model.Age.Value;
        if (model.StartDate.HasValue) _startDate = model.StartDate.Value;
        if (model.EndDate.HasValue) _endDate = model.EndDate.Value;

        if (model.Price.HasValue)
        {
            _totalPrice = model.Price.Value;
            WithPricesFromSetupModels(new List<PriceEpisodeSetupModel>
            {
                new PriceEpisodeSetupModel
                {
                    StartDate = _startDate,
                    EndDate = _endDate,
                    Price = (int)_totalPrice
                }
            });
        }

        return this;
    }

    public LearningCreatedEventBuilder WithPricesFromSetupModels(List<PriceEpisodeSetupModel> models)
    {
        _prices = models.Select(x => new LearningEpisodePrice
        {
            TotalPrice = x.Price,
            StartDate = x.StartDate,
            EndDate = x.EndDate
        }).ToList();

        _startDate = _prices.Min(x => x.StartDate);

        return this;
    }
    public LearningCreatedEventBuilder WithLearningKey(Guid key)
    {
        _learningKey = key;
        return this;
    }

    public LearningCreatedEventBuilder WithEmployerAccountId(long employerAccountId)
    {
        _employerAccountId = employerAccountId;
        return this;
    }

    public LearningCreatedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public LearningCreatedEventBuilder WithPriceKey(Guid priceKey)
    {
        _priceKey = priceKey;
        return this;
    }

    public LearningCreatedEventBuilder WithDuration(int months)
    {
        _endDate = _startDate.AddMonths(months);
        return this;
    }

    public LearningCreatedEvent Build()
    {
        return new LearningCreatedEvent
        {
            LearningKey = _learningKey,
            Uln = _uln,
            ApprovalsApprenticeshipId = _approvalsApprenticeshipId,
            DateOfBirth = CalculateDateOfBirth(_startDate, _ageAtStart),
            Episode = new LearningEpisode
            {
                Key = _episodeKey,
                FundingBandMaximum = _fundingBandMaximum,
                Prices = _prices.Any() ? _prices : new List<LearningEpisodePrice>
                {
                    new LearningEpisodePrice
                    {
                        Key = _priceKey,
                        TotalPrice = _totalPrice,
                        StartDate = _startDate,
                        EndDate = _endDate,
                    }
                },
                EmployerAccountId = _employerAccountId,
                FundingType = Learning.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = _fundingPlatform,
                AgeAtStartOfLearning = _ageAtStart
            }
        };
    }

    private static DateTime CalculateDateOfBirth(DateTime startDate, int age)
    {
        return startDate.AddYears(-age).AddDays(1);
    }
}