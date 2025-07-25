﻿using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class FundingTypeExtensions
{
    public static EmployerType ToOutboundEventEmployerType(this FundingType fundingType)
    {
        return fundingType == FundingType.NonLevy ? EmployerType.NonLevy : EmployerType.Levy;
    }
}