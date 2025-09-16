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

    public static PriceChangeSavePricesRequestBuilder GetPriceChangeSavePricesRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out PriceChangeSavePricesRequestBuilder builder)) return builder;

        builder = new PriceChangeSavePricesRequestBuilder();
        context.Set(builder);
        return builder;
    }

    public static StartDateSavePricesRequestBuilder GetStartDateSavePricesRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out StartDateSavePricesRequestBuilder builder)) return builder;

        builder = new StartDateSavePricesRequestBuilder();
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