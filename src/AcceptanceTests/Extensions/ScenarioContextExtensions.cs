using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Types;

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

    public static UpdateOnProgrammeRequestBuilder GetUpdateOnProgrammeRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out UpdateOnProgrammeRequestBuilder builder)) return builder;

        builder = new UpdateOnProgrammeRequestBuilder();
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