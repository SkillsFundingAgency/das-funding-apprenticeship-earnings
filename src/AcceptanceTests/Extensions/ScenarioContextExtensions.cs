using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

public static class ScenarioContextExtensions
{
    public static LearningCreatedEventBuilder GetLearningCreatedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out LearningCreatedEventBuilder builder)) return builder;

        builder = new LearningCreatedEventBuilder();
        context.Set(builder);
        return builder;
    }

    public static LearningPriceChangedEventBuilder GetLearningPriceChangedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out LearningPriceChangedEventBuilder builder)) return builder;

        builder = new LearningPriceChangedEventBuilder();
        context.Set(builder);
        return builder;
    }

    public static LearningPriceChangedRequestBuilder GetLearningPriceChangedRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out LearningPriceChangedRequestBuilder builder)) return builder;

        builder = new LearningPriceChangedRequestBuilder();
        context.Set(builder);
        return builder;
    }

    public static LearningStartDateChangedEventBuilder GetLearningStartDateChangedEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out LearningStartDateChangedEventBuilder builder)) return builder;

        builder = new LearningStartDateChangedEventBuilder();
        context.Set(builder);
        return builder;
    }

    public static LearningWithdrawnEventBuilder GetLearningWithdrawnEventBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out LearningWithdrawnEventBuilder builder)) return builder;

        builder = new LearningWithdrawnEventBuilder();
        context.Set(builder);
        return builder;
    }
}