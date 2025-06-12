using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public class ApprenticeshipStartDateChangedEventBuilder
{
    private Guid _apprenticeshipKey = Guid.NewGuid();
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
    private int _ageAtStartOfApprenticeship = 19;

    public ApprenticeshipStartDateChangedEventBuilder WithApprenticeshipKey(Guid key)
    {
        _apprenticeshipKey = key;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithApprenticeshipId(long id)
    {
        _apprenticeshipId = id;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithApprovedDate(DateTime date)
    {
        _approvedDate = date;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithProviderApprovedBy(string approvedBy)
    {
        _providerApprovedBy = approvedBy;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithEmployerApprovedBy(string approvedBy)
    {
        _employerApprovedBy = approvedBy;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithInitiator(string initiator)
    {
        _initiator = initiator;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithEpisodeKey(Guid episodeKey)
    {
        _episodeKey = episodeKey;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithPriceKey(Guid priceKey)
    {
        _priceKey = priceKey;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithFundingBandMaximum(int max)
    {
        _fundingBandMaximum = max;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithEmployerAccountId(long accountId)
    {
        _employerAccountId = accountId;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithAgeAtStart(int age)
    {
        _ageAtStartOfApprenticeship = age;
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithDuration(int months)
    {
        _endDate = _startDate.AddMonths(months);
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithAdjustedStartDateBy(int months)
    {
        _startDate = _startDate.AddMonths(months);
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder WithAdjustedEndDateBy(int months)
    {
        _endDate = _endDate.AddMonths(months);
        return this;
    }

    public ApprenticeshipStartDateChangedEventBuilder FromSetupModel(StartDateChangeModel model)
    {
        if (model.NewStartDate.HasValue) _startDate = model.NewStartDate.Value;
        if (model.ApprovedDate.HasValue) _approvedDate = model.ApprovedDate.Value;
        return this;
    }

    public ApprenticeshipStartDateChangedEvent Build()
    {
        return new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipKey,
            ApprenticeshipId = _apprenticeshipId,
            ApprovedDate = _approvedDate,
            ProviderApprovedBy = _providerApprovedBy,
            EmployerApprovedBy = _employerApprovedBy,
            Initiator = _initiator,
            StartDate = _startDate,
            Episode = new ApprenticeshipEpisode
            {
                Key = _episodeKey,
                Prices = new List<ApprenticeshipEpisodePrice>
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
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS
            }
        };
    }
}