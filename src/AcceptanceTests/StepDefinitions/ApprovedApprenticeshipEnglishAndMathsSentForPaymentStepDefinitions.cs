using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions
{
    [Binding]
    public class ApprovedApprenticeshipEnglishAndMathsSentForPaymentStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly TestContext _testContext;

        public ApprovedApprenticeshipEnglishAndMathsSentForPaymentStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
        {
            _scenarioContext = scenarioContext;
            _testContext = testContext;
        }

        [When("the earnings are released for the apprenticeship")]
        public async Task WhenTheEarningsAreReleasedForTheApprenticeship()
        {
            var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
            var releaseRequest = new ReleaseEarningsRequest
            {
                LearnerKey = _scenarioContext.GetLearnerKey(),
                LearnerRef = _scenarioContext.GetLearnerRef()
            };

            await _testContext.TestInnerApi.Post($"/learning/{request.LearningKey}/release-earnings", releaseRequest);
        }

        [Then("the payments event is sent to pv2 for the english and maths course with the correct information")]
        public async Task ThenThePaymentsEventIsSentToPv2ForTheEnglishAndMathsCourseWithTheCorrectInformation()
        {
            var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
            var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
            var domainModel = ApprenticeshipLearning.Get(dbEntity!);
            var episode = (ApprenticeshipEpisode)domainModel.GetEpisode(request.EpisodeKey);
            var employerAccountId = _scenarioContext.GetEmployerAccountId();
            var fundingAccountId = _scenarioContext.GetFundingAccountId();

            episode.EarningsProfile!.MathsAndEnglishCourses.Should().NotBeEmpty("this scenario expects an English & Maths course to have been persisted");

            var englishAndMathsEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .Where(e => e.Training.LearningType == LearningType.MathsAndEnglish)
                .ToList();

            englishAndMathsEvents.Should().NotBeEmpty();

            foreach (var course in episode.EarningsProfile.MathsAndEnglishCourses)
            {
                var learnAimRef = course.LearnAimRef.Trim();
                var paymentsEvent = englishAndMathsEvents.LastOrDefault(e => e.Training.CourseReference == learnAimRef);
                paymentsEvent.Should().NotBeNull($"expected an English & Maths payments event for {learnAimRef}");

                paymentsEvent!.EarningsId.Should().Be(episode.EarningsProfile.Version);
                paymentsEvent.UKPRN.Should().Be(episode.UKPRN);
                paymentsEvent.EmployerContribution.Should().Be(0);

                paymentsEvent.Learner.LearnerKey.Should().Be(_scenarioContext.GetLearnerKey());
                paymentsEvent.Learner.ULN.Should().Be(long.Parse(request.Learner.Uln));
                paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());

                paymentsEvent.Training.LearningKey.Should().Be(request.LearningKey);
                paymentsEvent.Training.CourseType.Should().Be(CourseType.FunctionalSkill);
                paymentsEvent.Training.LearningType.Should().Be(LearningType.MathsAndEnglish);
                paymentsEvent.Training.CourseCode.Should().Be(learnAimRef);
                paymentsEvent.Training.CourseReference.Should().Be(learnAimRef);
                paymentsEvent.Training.StartDate.Should().Be(course.StartDate);
                paymentsEvent.Training.PlannedEndDate.Should().Be(course.EndDate);

                var expectedTrainingStatus = TrainingStatus.Continuing;
                if (course.CompletionDate.HasValue) expectedTrainingStatus = TrainingStatus.Completed;
                if (course.WithdrawalDate.HasValue) expectedTrainingStatus = TrainingStatus.Withdrawn;
                paymentsEvent.Training.TrainingStatus.Should().Be(expectedTrainingStatus);

                var academicYears = course.Instalments.Select(x => x.AcademicYear).Distinct().ToList();
                paymentsEvent.Earnings.Should().HaveCount(academicYears.Count);

                foreach (var instalment in course.Instalments)
                {
                    var yearlyEarning = paymentsEvent.Earnings.SingleOrDefault(e => e.AcademicYear == instalment.AcademicYear);
                    yearlyEarning.Should().NotBeNull();

                    var expectedEarningType = instalment.Type switch
                    {
                        EnglishAndMathsInstalmentType.Regular => EarningType.OnProgrammeMathsAndEnglish,
                        EnglishAndMathsInstalmentType.Balancing => EarningType.BalancingMathsAndEnglish,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var period = yearlyEarning!.PricePeriods.SelectMany(p => p.Periods)
                        .SingleOrDefault(p => p.EarningType == expectedEarningType && p.DeliveryPeriod == instalment.DeliveryPeriod && p.Amount == instalment.Amount);
                    period.Should().NotBeNull($"expected a {expectedEarningType} earning period for delivery period {instalment.DeliveryPeriod}");

                    period!.LearningId.Should().Be(domainModel.ApprovalsApprenticeshipId);
                    period.Employer.AccountId.Should().Be(employerAccountId);
                    period.Employer.FundingAccountId.Should().Be(fundingAccountId);
                }
            }
        }
    }
}
