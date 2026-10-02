using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class ApprovedApprenticeshipSentForPaymentStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        public ApprovedApprenticeshipSentForPaymentStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }

        [Given("the learner has no LearnerRef")]
        public void GivenTheLearnerHasNoLearnerRef()
        {
            // Simulates a manual-add apprenticeship (route B): no prior ILR-led LearnerData record,
            // so no LearnerRef is ever set. Per FLP-2003, this must never reach Payments.
            _scenarioContext.SetNoLearnerRef();
        }

        [Then("the payments event is sent to pv2 with the correct information for the apprenticeship")]
        public async Task ThenThePaymentsEventIsSentToPv2WithTheCorrectInformationForTheApprenticeship()
        {
            var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
            var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
            var domainModel = ApprenticeshipLearning.Get(dbEntity!);
            var episode = (ApprenticeshipEpisode)domainModel.GetEpisode(request.EpisodeKey);
            var employerAccountId = _scenarioContext.GetEmployerAccountId();
            var fundingAccountId = _scenarioContext.GetFundingAccountId();

            var paymentsEvent = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>().LastOrDefault();
            paymentsEvent.Should().NotBeNull();

            paymentsEvent.EarningsId.Should().Be(episode.EarningsProfile!.Version);
            paymentsEvent.UKPRN.Should().Be(request.OnProgramme.Ukprn);
            paymentsEvent.EmployerContribution.Should().Be(0);

            paymentsEvent.Learner.Should().NotBeNull();
            paymentsEvent.Learner.LearnerKey.Should().Be(_scenarioContext.GetLearnerKey());
            paymentsEvent.Learner.ULN.Should().Be(long.Parse(request.Learner.Uln));
            paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());

            paymentsEvent.Training.Should().NotBeNull();
            paymentsEvent.Training.LearningKey.Should().Be(request.LearningKey);
            paymentsEvent.Training.CourseType.Should().Be(CourseType.Apprenticeship);
            paymentsEvent.Training.LearningType.Should().Be(LearningType.Apprenticeship);
            paymentsEvent.Training.CourseCode.Should().Be(request.OnProgramme.TrainingCode);
            paymentsEvent.Training.CourseReference.Should().Be("ZPROG0001");
            paymentsEvent.Training.AgeAtStartOfTraining.Should().Be((byte)episode.AgeAtStartOfApprenticeship);

            var expectedTrainingStatus = TrainingStatus.Continuing;
            if (episode.CompletionDate.HasValue) expectedTrainingStatus = TrainingStatus.Completed;
            if (episode.WithdrawalDate.HasValue) expectedTrainingStatus = TrainingStatus.Withdrawn;
            paymentsEvent.Training.TrainingStatus.Should().Be(expectedTrainingStatus);
            paymentsEvent.Training.ActualEndDate.Should().Be(episode.WithdrawalDate ?? episode.CompletionDate);

            var instalments = episode.EarningsProfile.Instalments;
            var academicYears = instalments.Select(x => x.AcademicYear).Distinct().ToList();

            paymentsEvent.Earnings.Should().NotBeNull();
            paymentsEvent.Earnings.Should().HaveCount(academicYears.Count);

            // AC1: on-programme (Learning), Completion and Balancing earning types are all present and correctly mapped
            var prices = episode.Prices.ToDictionary(p => p.PriceKey);
            foreach (var instalment in instalments)
            {
                var price = prices[instalment.EpisodePriceKey];
                var yearlyEarning = paymentsEvent.Earnings.SingleOrDefault(e => e.AcademicYear == instalment.AcademicYear);
                yearlyEarning.Should().NotBeNull();

                var pricePeriod = yearlyEarning!.PricePeriods.SingleOrDefault(p => p.Price == price.AgreedPrice && p.StartDate == price.StartDate && p.EndDate == price.EndDate);
                pricePeriod.Should().NotBeNull();

                var expectedEarningType = instalment.Type switch
                {
                    InstalmentType.Regular => EarningType.Learning,
                    InstalmentType.Completion => EarningType.Completion,
                    InstalmentType.Balancing => EarningType.Balancing,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var period = pricePeriod!.Periods.SingleOrDefault(p => p.EarningType == expectedEarningType && p.DeliveryPeriod == instalment.DeliveryPeriod && p.Amount == instalment.Amount);
                period.Should().NotBeNull($"expected a {expectedEarningType} earning period for delivery period {instalment.DeliveryPeriod}");

                period!.LearningId.Should().Be(domainModel.ApprovalsApprenticeshipId);
                period.Employer.AccountId.Should().Be(employerAccountId);
                period.Employer.FundingAccountId.Should().Be(fundingAccountId);
            }

            // AC1 - all three earning types are represented in this scenario (on-programme + completion date + achievement date)
            instalments.Select(x => x.Type).Distinct().Should().BeEquivalentTo(new[]
            {
                InstalmentType.Regular,
                InstalmentType.Completion,
                InstalmentType.Balancing
            });
        }

        [Then("no payments event is sent to pv2")]
        public void ThenNoPaymentsEventIsSentToPv2()
        {
            _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>().Should().BeEmpty();
        }
    }
}
