using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public async Task WhenAPauseDateOfIsSent(DateTime pauseDate)
    {
        var pauseRequest = new PauseRequest
        {
            PauseDate = pauseDate
        };
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/pause", pauseRequest);
    }
}
