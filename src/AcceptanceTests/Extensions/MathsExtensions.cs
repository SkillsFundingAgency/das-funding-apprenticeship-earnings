using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

public static class MathsExtensions
{
    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        return Math.Round(value, decimalPlaces);
    }
}
