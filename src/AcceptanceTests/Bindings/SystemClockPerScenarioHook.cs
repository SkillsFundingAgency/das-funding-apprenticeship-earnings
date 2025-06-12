namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Bindings;

[Binding]
public class SystemClockPerScenarioHook
{
    //Defaults the system clock, can be overridden if specific scenarios require a specific time.
    [BeforeScenario(Order = 1)]
    public void SetSystemClockToNow()
    {
        TestSystemClock.SetDateTime(DateTime.UtcNow);
    }
}