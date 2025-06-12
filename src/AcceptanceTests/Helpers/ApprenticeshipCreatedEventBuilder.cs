using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class ApprenticeshipCreatedEventBuilder
{
    private Guid _apprenticeshipKey = Guid.NewGuid();
    private string _uln = new Random().Next().ToString();
    private long _approvalsApprenticeshipId = EventBuilderSharedDefaults.ApprovalsApprenticeshipId;
    private DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private List<ApprenticeshipEpisodePrice> _prices = new();
    private DateTime _startDate = new DateTime(2019, 01, 01);
    private DateTime _endDate = new DateTime(2021, 1, 1);
    private int _ageAtStart = 21;
    private Apprenticeships.Enums.FundingPlatform _fundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS;
    private decimal _totalPrice = 15000m;
    private int _fundingBandMaximum = EventBuilderSharedDefaults.FundingBandMaximum;
    private long _employerAccountId = EventBuilderSharedDefaults.EmployerAccountId;
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceKey = Guid.NewGuid();

    public ApprenticeshipCreatedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithDateOfBirth(DateTime dob)
    {
        _dateOfBirth = dob;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithAgeAtStart(int age)
    {
        _ageAtStart = age;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithFundingPlatform(Apprenticeships.Enums.FundingPlatform platform)
    {
        _fundingPlatform = platform;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithPrices(List<ApprenticeshipEpisodePrice> prices)
    {
        _prices = prices;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithTotalPrice(decimal totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }
    public ApprenticeshipCreatedEventBuilder WithFundingBandMaximum(int fundingBandMaximum)
    {
        _fundingBandMaximum = fundingBandMaximum;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithDataFromSetupModel(ApprenticeshipCreatedSetupModel model)
    {
        if (model.Age.HasValue) _ageAtStart = model.Age.Value;
        
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithPricesFromSetupModels(List<PriceEpisodeSetupModel> models)
    {
        _prices = models.Select(x => new ApprenticeshipEpisodePrice
        {
            TotalPrice = x.Price,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            FundingBandMaximum = x.FundingBandMaximum ?? x.Price,
        }).ToList();
        return this;
    }
    public ApprenticeshipCreatedEventBuilder WithApprenticeshipKey(Guid key)
    {
        _apprenticeshipKey = key;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithEmployerAccountId(long employerAccountId)
    {
        _employerAccountId = employerAccountId;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithPriceKey(Guid priceKey)
    {
        _priceKey = priceKey;
        return this;
    }

    public ApprenticeshipCreatedEventBuilder WithDuration(int months)
    {
        _endDate = _startDate.AddMonths(months);
        return this;
    }

    public ApprenticeshipCreatedEvent Build()
    {
        return new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = _apprenticeshipKey,
            Uln = _uln,
            ApprovalsApprenticeshipId = _approvalsApprenticeshipId,
            DateOfBirth = _dateOfBirth,
            Episode = new ApprenticeshipEpisode
            {
                Key = _episodeKey,
                Prices = _prices.Any() ? _prices : new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        Key = _priceKey,
                        TotalPrice = _totalPrice,
                        StartDate = _startDate,
                        EndDate = _endDate,
                        FundingBandMaximum = _fundingBandMaximum
                    }
                },
                EmployerAccountId = _employerAccountId,
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = _fundingPlatform,
                AgeAtStartOfApprenticeship = _ageAtStart
            }
        };
    }
}