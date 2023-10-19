using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueueNames = SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.QueueNames;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private static IEndpointInstance? _endpointInstance;

    private PriceChangeApprovedEvent _priceChangeApprovedEvent;

    public RecalculateEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        _endpointInstance = await EndpointHelper
            .StartEndpoint(QueueNames.PriceChangeApproved, true, new[] { typeof(PriceChangeApprovedEvent) });
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    [Given("earnings have been calculated for an apprenticeship in the pilot")]
    public async Task PublishApprenticeshipCreatedEvent()
    {
        _priceChangeApprovedEvent = new PriceChangeApprovedEvent
        {

        };
        await _endpointInstance.Publish(_priceChangeApprovedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount] = 24;
        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 500;
    }
}
