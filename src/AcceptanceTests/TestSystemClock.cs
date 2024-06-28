using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class TestSystemClock : ISystemClock
{
    private static DateTime _testTime;

    public DateTimeOffset UtcNow => _testTime;

    public static void SetDateTime(DateTime dateTime)
    {
        _testTime = dateTime;
    }

    public static ISystemClock Instance()
    {
        return new TestSystemClock();
    }
}
