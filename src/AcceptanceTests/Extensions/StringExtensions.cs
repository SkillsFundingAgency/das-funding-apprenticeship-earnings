using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

internal static class StringExtensions
{
    internal static List<T> ToEnumList<T>(this string comaSeperatedValues) where T : struct, Enum
    {
        var enumList = new List<T>();

        if (!string.IsNullOrWhiteSpace(comaSeperatedValues))
        {
            var enumValues = comaSeperatedValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in enumValues)
            {
                if (Enum.TryParse(name.Trim(), out T milestone))
                {
                    enumList.Add(milestone);
                }
            }
        }

        return enumList;
    }
}
