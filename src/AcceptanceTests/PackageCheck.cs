using NUnit.Framework;
using SFA.DAS.Testing.PackageScanning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

[TestFixture]
public class PackageCheck
{
    [Test]
    [Ignore("Ignored so that this test will not run on the build server, uncomment to use this check locally.")]
    public void CheckForVulnerableAndDeprecatedPackages() =>
        PackageChecker.AssertNoVulnerableOrDeprecatedPackages(@"..\..\..\..\");
}