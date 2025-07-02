using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class LearningStartDateChangedEventBuilder
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

    public LearningStartDateChangedEventBuilder WithLearningKey(Guid key)
    {
        _learningKey = key;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithApprovedDate(DateTime date)
    {
        _approvedDate = date;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithProviderApprovedBy(string approvedBy)
    {
        _providerApprovedBy = approvedBy;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithEmployerApprovedBy(string approvedBy)
    {
        _employerApprovedBy = approvedBy;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithInitiator(string initiator)
    {
        _initiator = initiator;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithPriceKey(Guid priceKey)
    {
        _priceKey = priceKey;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithFundingBandMaximum(int max)
    {
        _fundingBandMaximum = max;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithEmployerAccountId(long accountId)
    {
        _employerAccountId = accountId;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithAgeAtStart(int age)
    {
        _ageAtStartOfLearning = age;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithDuration(int months)
    {
        _endDate = _startDate.AddMonths(months);
        return this;
    }

    public LearningStartDateChangedEventBuilder WithAdjustedStartDateBy(int months)
    {
        _startDate = _startDate.AddMonths(months);
        return this;
    }

    public LearningStartDateChangedEventBuilder WithAdjustedEndDateBy(int months)
    {
        _endDate = _endDate.AddMonths(months);
        return this;
    }

    public LearningStartDateChangedEventBuilder WithDataFromSetupModel(StartDateChangeModel model)
    {
        if (model.NewStartDate.HasValue) _startDate = model.NewStartDate.Value;
        if (model.ApprovedDate.HasValue) _approvedDate = model.ApprovedDate.Value;
        return this;
    }

    public LearningStartDateChangedEventBuilder WithExistingApprenticeshipData(LearningCreatedEvent apprenticeship)
    {
        _learningKey = apprenticeship.LearningKey;
        _episodeKey = apprenticeship.Episode.Key;
        _endDate = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().EndDate;
        _fundingBandMaximum = apprenticeship.Episode.Prices.OrderBy(x => x.StartDate).Last().FundingBandMaximum;
        _ageAtStartOfLearning = apprenticeship.Episode.AgeAtStartOfLearning;
        _apprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        return this;
    }

    public LearningStartDateChangedEvent Build()
    {
        return new LearningStartDateChangedEvent
        {
            LearningKey = _learningKey,
            ApprovalsApprenticeshipId = _apprenticeshipId,
            ApprovedDate = _approvedDate,
            ProviderApprovedBy = _providerApprovedBy,
            EmployerApprovedBy = _employerApprovedBy,
            Initiator = _initiator,
            StartDate = _startDate,
            Episode = new LearningEpisode
            {
                Key = _episodeKey,
                Prices = new List<LearningEpisodePrice>
                {
                    new()
                    {
                        Key = _priceKey,
                        StartDate = _startDate,
                        EndDate = _endDate,
                        TotalPrice = 15000,
                        FundingBandMaximum = _fundingBandMaximum
                    }
                },
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