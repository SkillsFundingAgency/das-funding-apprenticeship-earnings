using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

public static class ScenarioContextExtensions
{
    public static ApprenticeshipCreatedEventBuilder GetApprenticeshipCreatedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out ApprenticeshipCreatedEventBuilder builder)) return builder;

        builder = new ApprenticeshipCreatedEventBuilder();
        context.Set(builder);
        return builder;
    }
}