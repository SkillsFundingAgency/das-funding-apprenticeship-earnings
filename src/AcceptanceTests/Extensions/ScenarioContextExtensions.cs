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

    public static ApprenticeshipPriceChangedEventBuilder GetApprenticeshipPriceChangedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out ApprenticeshipPriceChangedEventBuilder builder)) return builder;

        builder = new ApprenticeshipPriceChangedEventBuilder();
        context.Set(builder);
        return builder;
    }

    public static ApprenticeshipStartDateChangedEventBuilder GetApprenticeshipStartDateChangedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out ApprenticeshipStartDateChangedEventBuilder builder)) return builder;

        builder = new ApprenticeshipStartDateChangedEventBuilder();
        context.Set(builder);
        return builder;
    }

    public static ApprenticeshipWithdrawnEventBuilder GetApprenticeshipWithdrawnEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out ApprenticeshipWithdrawnEventBuilder builder)) return builder;

        builder = new ApprenticeshipWithdrawnEventBuilder();
        context.Set(builder);
        return builder;
    }
}