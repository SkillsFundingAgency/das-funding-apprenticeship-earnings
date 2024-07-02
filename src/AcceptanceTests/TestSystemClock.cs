using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class TestSystemClock : ISystemClock, ISystemClockService
{
    private static DateTime _testTime;

    public DateTimeOffset UtcNow => _testTime;

    public static void SetDateTime(DateTime dateTime)
    {
        _testTime = dateTime;
    }

    public static ISystemClockService Instance()
    {
        return new TestSystemClock();
    }
}
