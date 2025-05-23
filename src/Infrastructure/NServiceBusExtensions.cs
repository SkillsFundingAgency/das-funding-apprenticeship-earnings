﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;

public static class NServiceBusExtensions
{
    public static void SetConventions(this ConventionsBuilder conventions)
    {
        conventions.DefiningEventsAs(IsEvent);
        conventions.DefiningCommandsAs(IsCommand);
        conventions.DefiningMessagesAs(IsMessage);
    }

    public static string GetFullyQualifiedNamespace(this string serviceBusConnectionString)
    {
        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
        {
            throw new ArgumentException("Service Bus connection string cannot be null or empty.", nameof(serviceBusConnectionString));
        }

        var parts = serviceBusConnectionString.Split(';');
        foreach (var part in parts)
        {
            if (part.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
            {
                var endpoint = part.Split('=')[1]; // Extract after "Endpoint="
                return new Uri(endpoint).Host; // Extract only the hostname
            }
        }

        throw new FormatException("Invalid Service Bus connection string: Fully Qualified Namespace not found.");
    }


    private static bool IsMessage(Type t) => t.Name.EndsWith("Message");

    private static bool IsEvent(Type t) => t.Name.EndsWith("Event");

    private static bool IsCommand(Type t) => t.Name.EndsWith("Command");

}
