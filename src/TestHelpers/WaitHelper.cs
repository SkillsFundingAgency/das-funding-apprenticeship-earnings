using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public static class WaitConfigurationHelper
{
    public static WaitConfiguration WaitConfiguration
    {
        get
        {
            if (_waitConfiguration == null)
            {
                _waitConfiguration = new WaitConfiguration();
                new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                    .Build()
                    .GetSection("WaitConfiguration")
                    .Bind(_waitConfiguration);
            }

            return _waitConfiguration;
        }
    }

    private static WaitConfiguration? _waitConfiguration;
}

public class WaitHelper
{
    private static WaitConfiguration Config => WaitConfigurationHelper.WaitConfiguration;

    public static async Task WaitForIt(Func<bool> lookForIt, string failText)
    {
        var endTime = DateTime.Now.Add(Config.TimeToWait);
        var lastRun = false;

        while (DateTime.Now < endTime || lastRun)
        {
            if (lookForIt())
            {
                if (lastRun) return;
                lastRun = true;
            }
            else
            {
                if (lastRun) break;
            }

            await Task.Delay(Config.TimeToPause);
        }
        Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
    }
}

public class WaitConfiguration
{
    public TimeSpan TimeToWait { get; set; }
    public TimeSpan TimeToPause { get; set; }
}