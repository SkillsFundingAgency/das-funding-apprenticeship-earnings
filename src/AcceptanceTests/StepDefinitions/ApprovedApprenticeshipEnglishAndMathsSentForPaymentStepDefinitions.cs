using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
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
        private const string PaymentsEventCountBeforeReleaseKey = "PaymentsEventCountBeforeRelease";

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
            await ReleaseEarnings(ReleaseType.All, []);
        }

        [When(@"the earnings are released with a release type of (.*)")]
        public async Task WhenTheEarningsAreReleasedWithAReleaseTypeOf(ReleaseType releaseType)
        {
            await ReleaseEarnings(releaseType, []);
        }

        [When("the english and maths earnings are released for the changed courses")]
        public async Task WhenTheEnglishAndMathsEarningsAreReleasedForTheChangedCourses()
        {
            // Mirrors the outer, which releases only the course keys returned from the english-and-maths update
            var response = _scenarioContext.Get<UpdateEnglishAndMathsResponseModel>(nameof(UpdateEnglishAndMathsResponseModel));
            response.Should().NotBeNull("the english-and-maths update should have returned a response");

            var courseKeys = response.GetUpdatedCourseKeys();
            courseKeys.Should().NotBeEmpty("the english-and-maths update should have reported the changed courses");

            await ReleaseEarnings(ReleaseType.FunctionalSkill, courseKeys);
        }

        [Then(@"only the english and maths course ""(.*)"" is sent to pv2 in the release")]
        public void ThenOnlyTheEnglishAndMathsCourseIsSentToPv2InTheRelease(string learnAimRef)
        {
            var releasedEvents = GetPaymentsEventsSentInRelease();

            releasedEvents.Should().ContainSingle("only the changed english and maths course should be released");
            releasedEvents.Single().Training.LearningType.Should().Be(LearningType.MathsAndEnglish);
            releasedEvents.Single().Training.CourseReference.Should().Be(learnAimRef);
        }

        [Then("no english and maths payments event is sent to pv2 in the release")]
        public void ThenNoEnglishAndMathsPaymentsEventIsSentToPv2InTheRelease()
        {
            var releasedEvents = GetPaymentsEventsSentInRelease();

            releasedEvents.Should().NotBeEmpty("the on-programme earnings should still be released");
            releasedEvents.Should().NotContain(e => e.Training.LearningType == LearningType.MathsAndEnglish);
        }

        [Then(@"the english and maths course ""(.*)"" is sent to pv2 with a new earnings version")]
        public async Task ThenTheEnglishAndMathsCourseIsSentToPv2WithANewEarningsVersion(string learnAimRef)
        {
            var course = await GetEnglishAndMathsCourse(learnAimRef);

            var courseEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .Where(e => e.Training.LearningType == LearningType.MathsAndEnglish && e.Training.CourseReference == learnAimRef)
                .ToList();

            courseEvents.Should().HaveCountGreaterThan(1, "the course should have been sent on approval and again after the change");
            courseEvents.Last().EarningsId.Should().Be(course.Version);
            courseEvents.Last().EarningsId.Should().NotBe(courseEvents.First().EarningsId);
        }

        [Then(@"the english and maths course ""(.*)"" is sent to pv2 with its previous earnings version")]
        public async Task ThenTheEnglishAndMathsCourseIsSentToPv2WithItsPreviousEarningsVersion(string learnAimRef)
        {
            var course = await GetEnglishAndMathsCourse(learnAimRef);

            var courseEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .Where(e => e.Training.LearningType == LearningType.MathsAndEnglish && e.Training.CourseReference == learnAimRef)
                .ToList();

            courseEvents.Should().ContainSingle("an unchanged course should only have been sent on approval");
            courseEvents.Single().EarningsId.Should().Be(course.Version);
        }

        [Then(@"the english and maths course ""(.*)"" is sent to pv2 as removed with no earnings")]
        public async Task ThenTheEnglishAndMathsCourseIsSentToPv2AsRemovedWithNoEarnings(string learnAimRef)
        {
            var course = await GetEnglishAndMathsCourse(learnAimRef);
            course.IsRemoved.Should().BeTrue("the course was omitted from the latest submission");

            var paymentsEvent = GetPaymentsEventsSentInRelease()
                .SingleOrDefault(e => e.Training.LearningType == LearningType.MathsAndEnglish && e.Training.CourseReference == learnAimRef);
            paymentsEvent.Should().NotBeNull($"expected the removed course {learnAimRef} to be sent so payments can clear its earnings");

            paymentsEvent!.EarningsId.Should().Be(course.Version);
            paymentsEvent.Earnings.Should().BeEmpty();
        }

        [Then(@"the english and maths course ""(.*)"" is not sent to pv2")]
        public async Task ThenTheEnglishAndMathsCourseIsNotSentToPv2(string learnAimRef)
        {
            var course = await GetEnglishAndMathsCourse(learnAimRef);
            course.IsRemoved.Should().BeTrue("the course was omitted from the latest submission");

            _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .Should().NotContain(e => e.Training.LearningType == LearningType.MathsAndEnglish && e.Training.CourseReference == learnAimRef);
        }

        private async Task ReleaseEarnings(ReleaseType releaseType, List<Guid> englishAndMathsCourseKeys)
        {
            var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
            var releaseRequest = new ReleaseEarningsRequest
            {
                LearnerKey = _scenarioContext.GetLearnerKey(),
                LearnerRef = _scenarioContext.GetLearnerRef(),
                ReleaseType = releaseType,
                EnglishAndMathsCourseKeys = englishAndMathsCourseKeys
            };

            _scenarioContext.Set(_testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>().Count, PaymentsEventCountBeforeReleaseKey);

            await _testContext.TestInnerApi.Post($"/learning/{request.LearningKey}/release-earnings", releaseRequest);
        }

        private List<CalculateGrowthAndSkillsPayments> GetPaymentsEventsSentInRelease()
        {
            var countBeforeRelease = _scenarioContext.Get<int>(PaymentsEventCountBeforeReleaseKey);

            return _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .Skip(countBeforeRelease)
                .ToList();
        }

        private async Task<SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths> GetEnglishAndMathsCourse(string learnAimRef)
        {
            var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
            var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
            var domainModel = ApprenticeshipLearning.Get(dbEntity!);
            var episode = (ApprenticeshipEpisode)domainModel.GetEpisode(request.EpisodeKey);

            return episode.EarningsProfile!.MathsAndEnglishCourses.Single(x => x.LearnAimRef.Trim() == learnAimRef);
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

            // Removed courses are asserted separately as they are only sent when released by course key
            foreach (var course in episode.EarningsProfile.MathsAndEnglishCourses.Where(x => !x.IsRemoved))
            {
                var learnAimRef = course.LearnAimRef.Trim();
                var paymentsEvent = englishAndMathsEvents.LastOrDefault(e => e.Training.CourseReference == learnAimRef);
                paymentsEvent.Should().NotBeNull($"expected an English & Maths payments event for {learnAimRef}");

                paymentsEvent!.EarningsId.Should().Be(course.Version);
                paymentsEvent.EarningsId.Should().NotBe(episode.EarningsProfile.Version);
                paymentsEvent.UKPRN.Should().Be(episode.UKPRN);
                paymentsEvent.EmployerContribution.Should().Be(0);

                paymentsEvent.Learner.LearnerKey.Should().Be(_scenarioContext.GetLearnerKey());
                paymentsEvent.Learner.ULN.Should().Be(long.Parse(request.Learner.Uln));
                paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());

                paymentsEvent.Training.LearningKey.Should().Be(course.Key);
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

                // Instalment amount and number of instalments are based on regular instalments only, not balancing
                var regularInstalments = course.Instalments.Where(x => x.Type == EnglishAndMathsInstalmentType.Regular).ToList();
                foreach (var pricePeriod in paymentsEvent.Earnings.SelectMany(e => e.PricePeriods))
                {
                    pricePeriod.Price.Should().Be(course.Amount);
                    pricePeriod.CompletionAmount.Should().Be(0);
                    pricePeriod.InstalmentAmount.Should().Be(regularInstalments.Count > 0 ? regularInstalments.Max(x => x.Amount) : 0);
                    pricePeriod.NumberOfInstalments.Should().Be(regularInstalments.Count);
                }

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
