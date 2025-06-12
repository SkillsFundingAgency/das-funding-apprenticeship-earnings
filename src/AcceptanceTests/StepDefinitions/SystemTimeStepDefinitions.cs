namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class SystemTimeStepDefinitions
{
    private readonly DateTime _defaultCurrentDateTime = new DateTime(2020, 01, 01);

    public SystemTimeStepDefinitions()
    {
        TestSystemClock.SetDateTime(_defaultCurrentDateTime);
    }

    [Given(@"the date is now (.*)")]
    public static void SetCurrentDate(string dateTime)
    {
        TestSystemClock.SetDateTime(DateTime.Parse(dateTime));
    }
}
