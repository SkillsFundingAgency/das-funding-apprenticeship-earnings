using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using System;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class PeriodInLearningHelper
{
    public static IPeriodInLearning Create(
        DateTime startDate,
        DateTime endDate,
        DateTime originalExpectedEndDate)
        => new Impl(startDate, endDate, originalExpectedEndDate);

    private sealed record Impl(
        DateTime StartDate,
        DateTime EndDate,
        DateTime OriginalExpectedEndDate
    ) : IPeriodInLearning;
}
