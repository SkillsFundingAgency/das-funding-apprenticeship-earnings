using NUnit.Framework;
using SFA.DAS.Testing.PackageScanning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

[TestFixture]
public class PackageCheck
{
    [Test]
    public void CheckForVulnerableAndDeprecatedPackages()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != "Development") return;

        PackageChecker.AssertNoVulnerableOrDeprecatedPackages(@"..\..\..\..\");
    }
}