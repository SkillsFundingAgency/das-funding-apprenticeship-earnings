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

    public static UpdateShortCourseOnProgrammeRequestBuilder GetShortCourseUpdateOnProgrammeRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out UpdateShortCourseOnProgrammeRequestBuilder builder)) return builder;

        builder = new UpdateShortCourseOnProgrammeRequestBuilder();
        context.Set(builder);
        return builder;
    }

    public static UpdateEnglishAndMathsRequestBuilder GetUpdateEnglishAndMathsRequestBuilder(this ScenarioContext context)
    {
        if (context.TryGetValue(out UpdateEnglishAndMathsRequestBuilder builder)) return builder;

        builder = new UpdateEnglishAndMathsRequestBuilder();
        context.Set(builder);

        return builder;
    }

    /// <summary>
    /// Returns learner key from the scenario context. If it does not exist, a new one is generated and stored in the context.
    /// </summary>
    /// <remarks>
    /// Only for use with single learner scenarios
    /// </remarks>
    public static Guid GetLearnerKey(this ScenarioContext context)
    {
        if(context.TryGetValue("LearnerKey", out Guid learnerKey)) return learnerKey;

        learnerKey = Guid.NewGuid();
        context.Set<Guid>(learnerKey, "LearnerKey");

        return learnerKey;
    }

    public static string GetLearnerRef(this ScenarioContext context)
    {
        if (context.TryGetValue("LearnerRef", out string learnerRef)) return learnerRef;
        learnerRef = $"LR-{Guid.NewGuid():N}";
        context.Set<string>(learnerRef, "LearnerRef");
        return learnerRef;
    }

    public static long GetApprovalsApprenticeshipId(this ScenarioContext context)
    {
        if (context.TryGetValue("ApprovalsApprenticeshipId", out long approvalsApprenticeshipId)) return approvalsApprenticeshipId;
        approvalsApprenticeshipId = new Random().Next(1, int.MaxValue);
        context.Set<long>(approvalsApprenticeshipId, "ApprovalsApprenticeshipId");
        return approvalsApprenticeshipId;
    }

    public static long GetEmployerAccountId(this ScenarioContext context)
    {
        if (context.TryGetValue("EmployerAccountId", out long employerAccountId)) return employerAccountId;
        employerAccountId = new Random().Next(1, int.MaxValue);
        context.Set<long>(employerAccountId, "EmployerAccountId");
        return employerAccountId;
    }

    public static long GetFundingAccountId(this ScenarioContext context)
    {
        if (context.TryGetValue("FundingAccountId", out long fundingAccountId)) return fundingAccountId;
        fundingAccountId = new Random().Next(1, int.MaxValue);
        context.Set<long>(fundingAccountId, "FundingAccountId");
        return fundingAccountId;
    }
}