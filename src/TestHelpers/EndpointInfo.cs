﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

public class EndpointInfo
{
    public string StarterName { get; private set; }
    public Dictionary<string, object> StarterArgs { get; private set; }

    public EndpointInfo(
        string starterName,
        Dictionary<string, object> args = null)
    {
        if (string.IsNullOrEmpty(starterName)) throw new ArgumentException("Missing starter name");

        StarterName = starterName;
        if (args == null)
        {
            args = new Dictionary<string, object>();
        }
        StarterArgs = args;
    }
}