namespace SFA.DAS.Funding.ApprenticeshipEarnings.Application;

public static class DateTimeExtensions
{
    public static byte ToDeliveryPeriod(this DateTime dateTime)
    {
        if (dateTime.Month >= 8)
            return (byte)(dateTime.Month - 7);
        else
            return (byte)(dateTime.Month + 5);
    }

    public static short ToAcademicYear(this DateTime dateTime)
    {
        var twoDigitYear = short.Parse(dateTime.Year.ToString().Substring(2));

        if (dateTime.Month >= 8)
            return short.Parse($"{twoDigitYear}{twoDigitYear + 1}");
        
        return short.Parse($"{twoDigitYear - 1}{twoDigitYear}");
    }
}