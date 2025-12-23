using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class DateOfBirthStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public DateOfBirthStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Then(@"date of birth is updated to (.*)")]
    public async Task ThenDateOfBirthIsUpdatedTo(DateTime dateOfBirth)
    {
        var learningKeyKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningKeyKey);
        updatedEntity.DateOfBirth.Should().Be(dateOfBirth);
    }
}
