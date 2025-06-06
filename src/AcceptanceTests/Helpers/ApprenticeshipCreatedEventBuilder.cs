using SFA.DAS.Apprenticeships.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers
{
    public class ApprenticeshipCreatedEventBuilder
    {
        private Guid _apprenticeshipKey = Guid.NewGuid();
        private string _uln = new Random().Next().ToString();
        private long _approvalsApprenticeshipId = ApprenticeshipCreatedEventDefaults.ApprovalsApprenticeshipId;
        private DateTime _dateOfBirth = ApprenticeshipCreatedEventDefaults.DateOfBirth;
        private List<ApprenticeshipEpisodePrice> _prices = new();
        private DateTime _startDate = ApprenticeshipCreatedEventDefaults.StartDate;
        private DateTime _endDate = ApprenticeshipCreatedEventDefaults.EndDate;
        private int _ageAtStart = ApprenticeshipCreatedEventDefaults.AgeAtStartOfApprenticeship;
        private Apprenticeships.Enums.FundingPlatform _fundingPlatform = ApprenticeshipCreatedEventDefaults.FundingPlatform;
        private decimal _totalPrice = ApprenticeshipCreatedEventDefaults.TotalPrice;
        private int _fundingBandMaximum = ApprenticeshipCreatedEventDefaults.FundingBandMaximum;

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

        public ApprenticeshipCreatedEventBuilder FromSetupModel(ApprenticeshipCreatedSetupModel model)
        {
            _ageAtStart = model.Age;
            return this;
        }

        public ApprenticeshipCreatedEventBuilder WithPricesFromSetupModels(List<PriceEpisodeSetupModel> models)
        {
            _prices = models.Select(x => new ApprenticeshipEpisodePrice
            {
                TotalPrice = x.Price,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                FundingBandMaximum = x.Price
            }).ToList();
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
                    Key = Guid.NewGuid(),
                    Prices = _prices.Any() ? _prices : new List<ApprenticeshipEpisodePrice>
                    {
                        new ApprenticeshipEpisodePrice
                        {
                            TotalPrice = _totalPrice,
                            StartDate = _startDate,
                            EndDate = _endDate,
                            FundingBandMaximum = _fundingBandMaximum
                        }
                    },
                    EmployerAccountId = 114,
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
}
