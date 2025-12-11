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

    public static List<T> ToList<T>(this KeyValueModel model) where T : new()
    {
        var list = new List<T>();
        list.Add(Parse<T>(model.Value));
        return list;
    }


    public static T Parse<T>(string input) where T : new()
    {
        var result = new T();
        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var kv = part.Split(':', 2, StringSplitOptions.TrimEntries);
            if (kv.Length != 2)
                continue;

            var propertyName = kv[0];
            var rawValue = kv[1];

            var prop = typeof(T).GetProperty(propertyName);
            if (prop == null || !prop.CanWrite)
                continue;

            var typedValue = ConvertTo(rawValue, prop.PropertyType);
            prop.SetValue(result, typedValue);
        }

        return result;
    }

    private static object ConvertTo(string value, Type targetType)
    {
        if (targetType == typeof(DateTime))
            return DateTime.Parse(value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value);

        return Convert.ChangeType(value, targetType);
    }
}

