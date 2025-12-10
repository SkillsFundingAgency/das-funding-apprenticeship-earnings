using Polly;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.BreakInLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class BreakInLearningStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public BreakInLearningStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When(@"a pause date of (.*) is sent")]
    [Given(@"a pause date of (.*) is sent")]
    public async Task WhenAPauseDateOfIsSent(DateTime pauseDate)
    {
        var pauseRequest = new PauseRequest
        {
            PauseDate = pauseDate
        };
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/pause", pauseRequest);
    }

    [When(@"a pause is removed")]
    public async Task WhenAPauseIsRemoved()
    {
        await _testContext.TestInnerApi.Delete($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/pause");
    }

    [When(@"SLD informs us that the break in learning was")]
    public async Task WhenSLDInformsUsThatTheBreakInLearningWas(Table table)
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var breaksInLearning = table.CreateSet<BreakInLearningModel>().ToList();

        var breaksInLearningRequest = new BreaksInLearningRequest();
        breaksInLearningRequest.EpisodeKey = learningCreatedEvent.Episode.Key;
        breaksInLearningRequest.BreaksInLearning = new List<BreakInLearningPeriod>();

        foreach (var breakInLearning in breaksInLearning)
        {
            breaksInLearningRequest.BreaksInLearning.Add(new BreakInLearningPeriod
            {
                StartDate = breakInLearning.StartDate,
                EndDate = breakInLearning.EndDate,
                PriorPeriodExpectedEndDate = breakInLearning.PriorPeriodExpectedEndDate
            });
        }

        await _testContext.TestInnerApi.Patch($"/apprenticeship/{learningCreatedEvent.LearningKey}/breaksInLearning ", breaksInLearningRequest);
    }
}
