using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Handlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class EarningsGeneratedEventHandlingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private static IEndpointInstance _endpointInstance;

    public EarningsGeneratedEventHandlingStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        _endpointInstance = await EndpointHelper
            .StartEndpoint("Test.Funding.ApprenticeshipEarnings", false, new[] { typeof(EarningsGeneratedEvent) });
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    [Then(@"Earnings are generated with the correct learning amounts")]
    public async Task AssertEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForIt(() => EarningsGeneratedEventHandler.ReceivedEvents.Any(EventMatchesExpectation), "Failed to find published EarningsGenerated event");
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent)
    {
        return earningsGeneratedEvent.FundingPeriods.Count == 1 
               && earningsGeneratedEvent.FundingPeriods.First().DeliveryPeriods.Count == (int)_scenarioContext["expectedDeliveryPeriodCount"]
               && earningsGeneratedEvent.FundingPeriods.First().DeliveryPeriods.All(x => x.LearningAmount == (int)_scenarioContext["expectedDeliveryPeriodLearningAmount"]);
    }
}