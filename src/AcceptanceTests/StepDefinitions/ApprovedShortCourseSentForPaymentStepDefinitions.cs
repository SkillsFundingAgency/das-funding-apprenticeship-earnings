using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class ApprovedShortCourseSentForPaymentStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        public ApprovedShortCourseSentForPaymentStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }

        [Then("the payments event is sent to pv2 with the correct information")]
        public async Task ThenThePaymentsEventIsSentToPv2WithTheCorrectInformation()
        {
            var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
            var dbEntity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
            var domainModel = ShortCourseLearning.Get(dbEntity);
            var episode = (ShortCourseEpisode)domainModel.GetEpisode(request.EpisodeKey);
            var approvalsApprenticeshipId = _scenarioContext.GetApprovalsApprenticeshipId();
            var employerAccountId = _scenarioContext.GetEmployerAccountId();
            var fundingAccountId = _scenarioContext.GetFundingAccountId();

            var paymentsEvent = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>().LastOrDefault();
            paymentsEvent.Should().NotBeNull();

            paymentsEvent.EarningsId.Should().Be(episode.EarningsProfile!.Version);
            paymentsEvent.UKPRN.Should().Be(request.OnProgramme.Ukprn);
            paymentsEvent.EmployerContribution.Should().Be(0); //todo not specified on design, assumption

            paymentsEvent.Learner.Should().NotBeNull();
            paymentsEvent.Learner.LearnerKey.Should().Be(_scenarioContext.GetLearnerKey());
            paymentsEvent.Learner.ULN.Should().Be(long.Parse(request.Learner.Uln));
            paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());

            paymentsEvent.Training.Should().NotBeNull();
            paymentsEvent.Training.LearningKey.Should().Be(request.LearningKey); //todo not specified on design, assumption
            paymentsEvent.Training.CourseType.Should().Be(CourseType.ShortCourse);
            paymentsEvent.Training.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
            paymentsEvent.Training.CourseCode.Should().Be(request.OnProgramme.CourseCode);
            paymentsEvent.Training.CourseReference.Should().Be(request.OnProgramme.CourseCode);
            paymentsEvent.Training.AgeAtStartOfTraining.Should().Be((byte)episode.AgeAtStartOfApprenticeship);
            paymentsEvent.Training.StartDate.Should().Be(episode.StartDate);
            paymentsEvent.Training.PlannedEndDate.Should().Be(episode.EndDate);
            paymentsEvent.Training.ActualEndDate.Should().Be(episode.WithdrawalDate ?? episode.CompletionDate);
            var expectedTrainingStatus = TrainingStatus.Continuing;
            if (episode.CompletionDate.HasValue) expectedTrainingStatus = TrainingStatus.Completed;
            if (episode.WithdrawalDate.HasValue) expectedTrainingStatus = TrainingStatus.Withdrawn;
            paymentsEvent.Training.TrainingStatus.Should().Be(expectedTrainingStatus);

            var academicYears = episode.EarningsProfile.Instalments.Where(x => x.IsPayable).Select(x => x.AcademicYear).Distinct().ToList();

            paymentsEvent.Earnings.Should().NotBeNull();
            paymentsEvent.Earnings.Should().HaveCount(academicYears.Count);

            foreach (var academicYear in academicYears)
            {
                var yearlyEarning = paymentsEvent.Earnings.SingleOrDefault(e => e.AcademicYear == academicYear);
                yearlyEarning.Should().NotBeNull($"Expected to find an earning for academic year {academicYear}, but none was found.");

                yearlyEarning.PricePeriods.Should().NotBeNull();
                yearlyEarning.PricePeriods.Should().HaveCount(1);

                var pricePeriod = yearlyEarning.PricePeriods.Single();
                pricePeriod.Price.Should().Be(request.OnProgramme.TotalPrice);
                pricePeriod.StartDate.Should().Be(episode.StartDate);
                pricePeriod.EndDate.Should().Be(episode.EndDate);
                pricePeriod.Periods.Should().NotBeNull();

                pricePeriod.Periods.Should().HaveCount(episode.EarningsProfile.Instalments.Count(x => x.IsPayable));

                AssertPricePeriod(academicYear, pricePeriod.Periods, EarningType.Milestone1, episode, approvalsApprenticeshipId, employerAccountId, fundingAccountId);
                AssertPricePeriod(academicYear, pricePeriod.Periods, EarningType.Completion, episode, approvalsApprenticeshipId, employerAccountId, fundingAccountId);
            }
        }

        private void AssertPricePeriod(
            short academicYear,
            IEnumerable<EarningPeriod> period, 
            EarningType earningType, 
            ShortCourseEpisode episode,
            long approvalsApprenticeshipId,
            long employerAccountId,
            long fundingAccountId)
        {
            var expectedType = earningType == EarningType.Milestone1 ? 
                ShortCourseInstalmentType.ThirtyPercentLearningComplete : ShortCourseInstalmentType.LearningComplete;

            var expectedInstalment = episode.EarningsProfile.Instalments.SingleOrDefault(x => x.IsPayable && x.Type == expectedType && x.AcademicYear == academicYear);

            var learningPeriod = period.SingleOrDefault(p => p.EarningType == earningType);

            if(expectedInstalment != null && learningPeriod == null)
            {
                learningPeriod.Should().NotBeNull($"Expected to find a {earningType} earning period, but none was found.");
            }

            if(expectedInstalment == null && learningPeriod != null)
            {
                learningPeriod.Should().BeNull($"Did not expect to find a {earningType} earning period, but one was found.");
            }

            if(expectedInstalment == null && learningPeriod == null)
            {
                return; // nothing to assert
            }

            learningPeriod.Amount.Should().Be(expectedInstalment.Amount);
            learningPeriod.DeliveryPeriod.Should().Be(expectedInstalment.DeliveryPeriod);
            learningPeriod.LearningId.Should().Be(approvalsApprenticeshipId);
            learningPeriod.Employer.AccountId.Should().Be(employerAccountId);
            learningPeriod.Employer.FundingAccountId.Should().Be(fundingAccountId);
            learningPeriod.Employer.EmployerType.Should().Be(EmployerType.Levy);
        }
    }
}
