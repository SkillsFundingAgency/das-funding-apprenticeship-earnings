using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using LearningEmployerType = SFA.DAS.Learning.Enums.EmployerType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class LearningEmployerTypeExtensions
{
    public static EmployerType ToEmployerType(this LearningEmployerType employerType)
    {
        return employerType == LearningEmployerType.Levy ? EmployerType.Levy : EmployerType.NonLevy;
    }
}
