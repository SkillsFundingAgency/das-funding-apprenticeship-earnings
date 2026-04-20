using System.Security.Cryptography;
using System.Text;

namespace SFA.DAS.ServiceBus.Implementation;

internal static class AzureRuleNameShortener
{
    private const int MaxLength = 50;

    internal static string Shorten(Type type)
    {
        var fullName = type.FullName!;
        // Option 1: Full name
        if (fullName.Length <= MaxLength)
            return fullName;

        var shortName = type.Name;
        var hash = GetHash(fullName);

        // Option 2: short name + hash suffix
        var candidate = $"{shortName}-{hash}";
        if (candidate.Length <= MaxLength)
            return candidate;

        // Option 3: acronym fallback + hash suffix
        var acronym = GetAcronym(shortName);
        return $"{acronym}-{hash}";
    }

    private static string GetHash(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..6]; // short deterministic hash
    }

    private static string GetAcronym(string name)
    {
        // ApprenticeshipCreatedEvent → ACE
        return string.Concat(
            name
                .Split(new[] { '.', '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => char.ToUpperInvariant(p[0]))
        );
    }
}
