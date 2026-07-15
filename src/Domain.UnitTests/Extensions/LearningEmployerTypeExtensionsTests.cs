using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using LearningEmployerType = SFA.DAS.Learning.Enums.EmployerType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Extensions;

[TestFixture]
public class LearningEmployerTypeExtensionsTests
{
    [TestCase(LearningEmployerType.Levy, EmployerType.Levy)]
    [TestCase(LearningEmployerType.NonLevy, EmployerType.NonLevy)]
    public void ToEmployerType_MapsByName_NotByUnderlyingValue(LearningEmployerType employerType, EmployerType expected)
    {
        employerType.ToEmployerType().Should().Be(expected);
    }
}
