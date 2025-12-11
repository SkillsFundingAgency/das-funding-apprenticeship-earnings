using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class KeyValueModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public static class KeyValueModelExtensions
{
    public static DateTime? ToNullableDateTime(this KeyValueModel model)
    {
        if (DateTime.TryParse(model.Value, out var date))
        {
            return date;
        }
        return null;
    }

    public static DateTime ToDateTime(this KeyValueModel model)
    {
        return DateTime.Parse(model.Value);
    }

    public static decimal? ToNullableDecimalValue(this KeyValueModel model)
    {
        if (decimal.TryParse(model.Value, out var decimalValue))
        {
            return decimalValue;
        }
        return null;
    }

    public static decimal ToDecimalValue(this KeyValueModel model)
    {
        return decimal.Parse(model.Value);
    }
}