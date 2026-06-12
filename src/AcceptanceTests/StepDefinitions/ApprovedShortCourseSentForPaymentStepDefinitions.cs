using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
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
            var episode = domainModel.GetEpisode();
            var learningApprovedEvent = _scenarioContext.Get<LearningApprovedEvent>();

            var paymentsEvent = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>().SingleOrDefault();
            paymentsEvent.Should().NotBeNull();

            paymentsEvent.EarningsId.Should().Be(episode.EarningsProfile!.EarningsProfileId);
            paymentsEvent.UKPRN.Should().Be(request.OnProgramme.Ukprn);
            paymentsEvent.EmployerContribution.Should().Be(0); //todo not specified on design, assumption

            paymentsEvent.Learner.Should().NotBeNull();
            paymentsEvent.Learner.LearnerKey.Should().Be(request.LearningKey);
            paymentsEvent.Learner.ULN.Should().Be(long.Parse(request.Learner.Uln));
            paymentsEvent.Learner.Reference.Should().Be(request.LearningKey.ToString()); //todo not specified on design, assumption

            paymentsEvent.Training.Should().NotBeNull();
            paymentsEvent.Training.LearningKey.Should().Be(request.LearningKey); //todo not specified on design, assumption
            paymentsEvent.Training.CourseType.Should().Be(CourseType.ShortCourse);
            paymentsEvent.Training.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
            paymentsEvent.Training.CourseCode.Should().Be(request.OnProgramme.CourseCode);
            paymentsEvent.Training.CourseReference.Should().Be($"ZSC{int.Parse(request.OnProgramme.CourseCode):D5}");
            paymentsEvent.Training.AgeAtStartOfTraining.Should().Be((byte)episode.AgeAtStartOfApprenticeship);
            paymentsEvent.Training.StartDate.Should().Be(episode.StartDate);
            paymentsEvent.Training.PlannedEndDate.Should().Be(episode.EndDate);
            paymentsEvent.Training.ActualEndDate.Should().BeNull(); //todo assert for completion, withdrawal etc. - Is this in scope for 1531 or just approval?
            paymentsEvent.Training.TrainingStatus.Should().Be(TrainingStatus.Continuing); //todo assert for completion, withdrawal etc. - Is this in scope for 1531 or just approval?

            paymentsEvent.Earnings.Should().NotBeNull();
            paymentsEvent.Earnings.Should().HaveCount(1);

            var yearlyEarning = paymentsEvent.Earnings.Single();
            yearlyEarning.AcademicYear.Should().Be(2021);

            yearlyEarning.PricePeriods.Should().NotBeNull();
            yearlyEarning.PricePeriods.Should().HaveCount(1);

            var pricePeriod = yearlyEarning.PricePeriods.Single();
            pricePeriod.Price.Should().Be(request.OnProgramme.TotalPrice);
            pricePeriod.StartDate.Should().Be(episode.StartDate);
            pricePeriod.EndDate.Should().Be(episode.EndDate);

            pricePeriod.Periods.Should().NotBeNull();
            pricePeriod.Periods.Should().HaveCount(2);

            var learningPeriod = pricePeriod.Periods.Single(p => p.EarningType == EarningType.Learning);
            learningPeriod.Amount.Should().Be(600);
            learningPeriod.DeliveryPeriod.Should().Be(7);
            learningPeriod.LearningId.Should().Be(domainModel.ApprovalsApprenticeshipId); //todo this is listed as "TBC" on the tech design
            learningPeriod.Employer.AccountId.Should().Be(learningApprovedEvent.EmployerAccountId);
            learningPeriod.Employer.FundingAccountId.Should().Be(learningApprovedEvent.FundingAccountId);
            learningPeriod.Employer.EmployerType.Should().Be(EmployerType.Levy);

            var completionPeriod = pricePeriod.Periods.Single(p => p.EarningType == EarningType.Completion);
            completionPeriod.Amount.Should().Be(1400);
            completionPeriod.DeliveryPeriod.Should().Be(11);
            completionPeriod.LearningId.Should().Be(domainModel.ApprovalsApprenticeshipId); //todo this is listed as "TBC" on the tech design
            completionPeriod.Employer.AccountId.Should().Be(learningApprovedEvent.EmployerAccountId);
            completionPeriod.Employer.FundingAccountId.Should().Be(learningApprovedEvent.FundingAccountId);
            completionPeriod.Employer.EmployerType.Should().Be(EmployerType.Levy);
        }
    }
}
