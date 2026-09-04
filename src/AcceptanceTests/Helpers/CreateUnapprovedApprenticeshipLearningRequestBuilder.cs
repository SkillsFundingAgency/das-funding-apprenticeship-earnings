using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class CreateUnapprovedApprenticeshipLearningRequestBuilder
{
    private Guid _learningKey = Guid.NewGuid();
    private string _uln = new Random().Next().ToString();
    private long _approvalsApprenticeshipId = EventBuilderSharedDefaults.ApprovalsApprenticeshipId;
    private List<LearningEpisodePrice> _prices = new();
    private DateTime _startDate = new DateTime(2019, 01, 01);
    private DateTime _endDate = new DateTime(2021, 1, 1);
    private int _ageAtStart = 21;
    private decimal _totalPrice = 15000m;
    private decimal _trainingPrice = 12000m;
    private decimal _epaPrice = 3000m;
    private long _employerAccountId = EventBuilderSharedDefaults.EmployerAccountId;
    private Guid _episodeKey = Guid.NewGuid();
    private Guid _priceKey = Guid.NewGuid();

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithAgeAtStart(int age)
    {
        _ageAtStart = age;
        return this;
    }

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithPrices(List<LearningEpisodePrice> prices)
    {
        _prices = prices;
        return this;
    }

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithDataFromSetupModel(ApprenticeshipCreatedSetupModel model)
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

    public CreateUnapprovedApprenticeshipLearningRequestBuilder WithPricesFromSetupModels(List<PriceEpisodeSetupModel> models)
    {
        _prices = models.Select(x => new LearningEpisodePrice
        {
            Key = Guid.NewGuid(),
            TotalPrice = x.Price,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            TrainingPrice = x.Price * 0.8m,
            EndPointAssessmentPrice = x.Price * 0.2m
        }).ToList();

        _startDate = _prices.Min(x => x.StartDate);

        return this;
    }

    public CreateUnapprovedApprenticeshipLearningRequest Build(int? fundingBandMaximum)
    {
        var prices = _prices.Any() ? _prices : new List<LearningEpisodePrice>
        {
            new LearningEpisodePrice
            {
                Key = _priceKey,
                TotalPrice = _totalPrice,
                StartDate = _startDate,
                EndDate = _endDate,
                TrainingPrice = _trainingPrice,
                EndPointAssessmentPrice = _epaPrice
            }
        };

        return new CreateUnapprovedApprenticeshipLearningRequest
        {
            LearningKey = _learningKey,
            EpisodeKey = _episodeKey,
            ApprovalsApprenticeshipId = _approvalsApprenticeshipId,
            Learner = new DraftApprenticeshipLearner
            {
                DateOfBirth = CalculateDateOfBirth(_startDate, _ageAtStart),
                Uln = _uln,
                Care = new DraftCare()
            },
            OnProgramme = new DraftApprenticeshipOnProgramme
            {
                TrainingCode = "AbleSeafarer",
                Ukprn = 116,
                EmployerAccountId = _employerAccountId,
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                EmployerType = EmployerType.Levy,
                FundingBandMaximum = fundingBandMaximum
            },
            Prices = prices,
            PeriodsInLearning = prices
                .Select(price => new ApprenticeshipPeriodInLearningItem
                {
                    StartDate = price.StartDate,
                    EndDate = null,
                    OriginalExpectedEndDate = price.EndDate
                })
                .ToList()
        };
    }

    private static DateTime CalculateDateOfBirth(DateTime startDate, int age)
    {
        return startDate.AddYears(-age).AddDays(1);
    }
}
